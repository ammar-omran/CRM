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
import { MatSelectModule } from '@angular/material/select';
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
    MatSelectModule,
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

  constructor(
    private fb: FormBuilder,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {
    this.buildForm();
    this.fetchDynamicOptions();
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
      this.filterChanged.emit(this.filterForm.value);
    }
  }

  resetForm() {
    this.filterForm.reset();
  }

  displayFn(control: FilterControl): (value: any) => string {
    return (value: any) => {
      const found = control.options?.find(opt => opt.value === value);
      return found?.label ?? '';
    };
  }
}
