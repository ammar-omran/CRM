import { CommonModule } from '@angular/common';
import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  ViewEncapsulation,
} from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatButtonModule } from '@angular/material/button';
import { MatOptionModule } from '@angular/material/core';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { TranslatePipe } from '@ngx-translate/core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { ApiService } from '@shared/services/api.service';
import { Observable, of } from 'rxjs';
import { debounceTime, switchMap } from 'rxjs/operators';

@Component({
  selector: 'app-filter',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    MatOptionModule,
    MatAutocompleteModule,
    MatDatepickerModule,
    MatButtonModule,
    MatIconModule,
    MatCheckboxModule,
    MatSelectModule,
    TranslatePipe,
  ],
  templateUrl: './filter.component.html',
  styleUrls: ['./filter.component.scss'],
  encapsulation: ViewEncapsulation.Emulated,
})
export class FilterComponent implements OnInit {
  @Input() controls: FilterControl[] = [];
  @Input() resetBtn: boolean = false;
  @Input() isPrimary: boolean = true;
  @Input() showAllOption: boolean = false;
  @Output() filterChanged: EventEmitter<any> = new EventEmitter<any>();

  filterForm!: FormGroup;
  userSearchResults$: Observable<any[]> = of([]);
  activeCount = 0;
  activeChips: Array<{ key: string; label: string; value: string }> = [];

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.fetchDynamicOptions();
    this.filterForm.valueChanges.subscribe(() => this.updateChips());
  }

  private updateChips(): void {
    const chips: Array<{ key: string; label: string; value: string }> = [];
    let count = 0;
    for (const c of this.controls) {
      const v = this.filterForm.get(c.formControlName)?.value;
      const isEmpty =
        v === null ||
        v === undefined ||
        v === '' ||
        v === '0' ||
        (Array.isArray(v) && v.length === 0);
      if (!isEmpty) {
        count++;
        let display = '';
        if (Array.isArray(v)) {
          if (c.options?.length) {
            display = v
              .map((id: any) => c.options?.find(o => String(o.value) === String(id))?.label ?? String(id))
              .join(', ');
          } else display = `${v.length} selected`;
        } else if (c.type === 'select' && c.options?.length) {
          display = c.options.find(o => String(o.value) === String(v))?.label ?? String(v);
        } else if (c.type === 'date' && v) {
          try { display = new Date(v).toLocaleDateString(); } catch { display = String(v); }
        } else display = String(v);
        if (display.length > 40) display = display.slice(0, 40) + '…';
        chips.push({ key: c.formControlName, label: c.label, value: display });
      }
    }
    this.activeCount = count;
    this.activeChips = chips;
  }

  clearOne(key: string): void {
    this.filterForm.get(key)?.setValue('');
    this.onSubmit();
  }

  fetchDynamicOptions(): void {
    this.controls.forEach(control => {
      const controlInstance = this.filterForm.get(control.formControlName);
      if (control.apiEndpoint && control.type === 'autocomplete') {
        controlInstance?.valueChanges
          .pipe(
            debounceTime(1000),
            switchMap((input: string) =>
              this.apiService.triggerApiRequest(
                `${control.apiEndpoint}?keyword=${input}` as EndPoint,
                HttpVerb.GET
              )
            )
          )
          .subscribe((response: any) => {
            control.options = (response ?? []).map((item: any) => ({
              label: item?.userName,
              value: item?.id,
            }));
          });
      } else if (control.apiEndpoint) {
        this.apiService.triggerApiRequest(control.apiEndpoint, HttpVerb.GET).subscribe({
          next: (response: any) => {
            if (typeof response[0] === 'string' && Array.isArray(response)) {
              control.options = response.map(item => ({
                value: item,
                label: item,
              }));
            } else if (response?.data && Array.isArray(response.data)) {
              control.options = response.data.map((item: any) => ({
                value: item.id,
                label: item.name,
              }));
            }

            if (control.isFirstValueDynamic) {
              if (control.options && control.options.length > 0) {
                const initialValue = control.options[0].value;
                this.filterForm.get(control.formControlName)?.setValue(initialValue);
                this.filterChanged.emit(this.filterForm.value);
              }
            } else {
              // set default selected option
              this.filterForm.get(control.formControlName)?.setValue('0');
              this.filterChanged.emit(this.filterForm.value);
            }
          },
          error: () => {
            console.error(`Error fetching dynamnic options form control ...`);
          },
        });
      }
    });
  }

  buildForm() {
    const formGroup: { [key: string]: any } = {};

    this.controls.forEach(control => {
      formGroup[control.formControlName] = [control.initialValue || '', control.validators || []];
    });

    this.filterForm = this.fb.group(formGroup);
  }

  onSubmit() {
    if (this.filterForm.valid) {
      this.updateChips();
      this.filterChanged.emit(this.filterForm.value);
    }
  }

  resetForm() {
    this.filterForm.reset();
    this.activeCount = 0;
    this.activeChips = [];
    this.filterChanged.emit(this.filterForm.value);
  }

  displayFn(control: FilterControl): (value: any) => string {
    return (value: any) => {
      const found = control.options?.find(opt => opt.value === value);
      return found?.label ?? '';
    };
  }
}
