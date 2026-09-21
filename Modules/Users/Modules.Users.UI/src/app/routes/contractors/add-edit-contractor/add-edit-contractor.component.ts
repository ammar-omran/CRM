import { AsyncPipe } from '@angular/common';
import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { BaseResponse } from '@shared/interfaces/base-response';
import { Contractor } from '@shared/interfaces/contractor.model';
import { District } from '@shared/interfaces/district.model';
import { ApiService } from '@shared/services/api.service';
import { StringValidator } from '@shared/validators/is-empty-string';
import { ToastrService } from 'ngx-toastr';
import { map, Observable, Subscription } from 'rxjs';

@Component({
  selector: 'app-add-edit-contractor',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslateModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    AsyncPipe,
    MatSelectModule,
  ],

  templateUrl: './add-edit-contractor.component.html',
  styleUrl: './add-edit-contractor.component.scss',
})
export class AddEditContractorComponent implements OnInit, OnDestroy {
  contractorForm!: FormGroup;
  isEditMode = false;
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  contractorId: string | null = null;
  availableDistricts$!: Observable<District[]>;
  private routeSub!: Subscription;
  private contractorsSub!: Subscription;
  private formSub!: Subscription;

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      this.contractorId = params.get('id');
      this.isEditMode = !!this.contractorId;
      this.initializeForm();
      if (this.isEditMode) {
        this.loadContractorData(this.contractorId!);
      }
    });

    this.availableDistricts$ = this.apiService
      .triggerApiRequest<BaseResponse<District[]>>(EndPoint.DISTRICTS, HttpVerb.GET)
      .pipe(map(response => response.data));
  }
  ngOnDestroy(): void {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
    if (this.contractorsSub) {
      this.contractorsSub.unsubscribe();
    }
    if (this.formSub) {
      this.formSub.unsubscribe();
    }
  }

  private initializeForm(): void {
    this.contractorForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(50), StringValidator.isEmptyString]],
      contactPersonName: [
        '',
        [Validators.required, Validators.maxLength(50), StringValidator.isEmptyString],
      ],
      phone: ['', Validators.required, StringValidator.isEmptyString],
      ghanaPostAddress: ['', Validators.maxLength(50)],
      officeAddress: ['', Validators.maxLength(250)],
      assignedDistricts: [[], Validators.required],
    });
    if (this.isEditMode) {
      this.contractorForm.addControl(
        'code',
        this.fb.control({ value: '', disabled: true }, Validators.required)
      );
    }
  }

  private loadContractorData(id: string): void {
    this.contractorsSub = this.apiService
      .triggerApiRequest<
        BaseResponse<Contractor>
      >(EndPoint.GET_CONTRACTOR_BY_ID, HttpVerb.GET, { id })
      .subscribe({
        next: response => {
          if (response && response.status.code === 0 && response.data) {
            const contractor = response.data;

            this.contractorForm.patchValue({
              name: contractor.name || '',
              contactPersonName: contractor.contactPersonName || '',
              phone: contractor.phone || '',
              ghanaPostAddress: contractor.ghanaPostAddress || '',
              officeAddress: contractor.officeAddress || '',
              assignedDistricts: contractor.assignedDistricts || [],
            });

            if (this.isEditMode && contractor.code) {
              this.contractorForm.controls.code.setValue(contractor.code);
            }
          } else {
            this.toastr.error('Failed to load contractor data');
          }
        },
        error: () => {
          this.toastr.error('An error occurred while fetching contractor data');
        },
      });
  }

  onSubmit(): void {
    if (this.contractorForm.invalid) return;

    const formData = this.contractorForm.value;

    if (this.isEditMode) {
      const updatedData = { ...formData, id: this.contractorId };
      this.formSub = this.apiService
        .triggerApiRequest(EndPoint.UPDATE_CONTRACTOR, HttpVerb.PUT, null, updatedData)
        .subscribe({
          next: () => {
            this.toastr.success('Contractor updated successfully');
            this.router.navigate(['/contractors']);
          },
        });
    } else {
      this.formSub = this.apiService
        .triggerApiRequest(EndPoint.ADD_CONTRACTOR, HttpVerb.POST, null, formData)
        .subscribe({
          next: () => {
            this.toastr.success('Contractor added successfully');
            this.router.navigate(['/contractors']);
          },
        });
    }
  }
}
