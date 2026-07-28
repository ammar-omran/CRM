import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { BaseResponse } from '@shared/interfaces/base-response';

@Component({
  selector: 'app-assign-customer-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatButtonModule,
    TranslateModule,
  ],
  templateUrl: './assign-customer-dialog.component.html',
  styleUrl: './assign-customer-dialog.component.scss',
})
export class AssignCustomerDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject<MatDialogRef<AssignCustomerDialogComponent>>(MatDialogRef);
  private apiService = inject(ApiService);

  form: FormGroup = this.fb.group({
    customers: [[], [Validators.required]],
  });

  availableCustomers: { id: number | string; email: string }[] = [];

  ngOnInit(): void {
    this.apiService
      .triggerApiRequest<{
        items: { id: number | string; email: string }[];
        totalCount: number;
      }>(EndPoint.CUSTOMERS_LIST, HttpVerb.GET, { Limit: 100 })
      .subscribe({
        next: response => {
          this.availableCustomers = response?.items ?? [];
        },
      });
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const customers = this.form.value.customers as { id: number | string; email: string }[];
    this.dialogRef.close(
      customers.map(c => ({
        id: c.id,
        email: c.email,
      }))
    );
  }

  cancel(): void {
    this.dialogRef.close();
  }
}
