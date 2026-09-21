import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { NgClass } from '@angular/common';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { BaseResponse } from '@shared/interfaces/base-response';
import { CustomValidators } from '@shared/utils/custom-validators';

@Component({
  selector: 'app-set-password',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslatePipe,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    NgClass,
  ],
  templateUrl: './set-password.component.html',
  styleUrl: './set-password.component.scss',
})
export class SetPasswordComponent implements OnInit {
  setPasswordForm!: FormGroup;
  private fb = inject(FormBuilder);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);

  token = '';
  isSubmitting = false;
  passwordCriteria = {
    minLength: false,
    uppercase: false,
    lowercase: false,
    specialCharacter: false,
  };

  ngOnInit(): void {
    this.token = this.route.snapshot.queryParams['token'] || '';

    this.setPasswordForm = this.fb.group({
      newPassword: ['', [Validators.required, CustomValidators.strongPassword()]],
      confirmPassword: [
        '',
        [Validators.required, CustomValidators.confirmPassword('newPassword')],
      ],
    });

    this.setPasswordForm.get('newPassword')?.valueChanges.subscribe(() => {
      this.updatePasswordChecklist();
    });
  }

  updatePasswordChecklist(): void {
    const password = this.setPasswordForm.get('newPassword')?.value || '';
    this.passwordCriteria.minLength = password.length >= 8;
    this.passwordCriteria.uppercase = /[A-Z]/.test(password);
    this.passwordCriteria.lowercase = /[a-z]/.test(password);
    this.passwordCriteria.specialCharacter = /[\W_]/.test(password);
  }

  allCriteriaMet(): boolean {
    return Object.values(this.passwordCriteria).every(value => value === true);
  }

  onSubmit(): void {
    if (this.setPasswordForm.invalid) return;

    this.isSubmitting = true;

    const formData = {
      Token: this.token,
      Password: this.setPasswordForm.value.newPassword,
    };

    this.apiService
      .triggerApiRequest<BaseResponse<null>>(EndPoint.SET_PASSWORD, HttpVerb.POST, undefined, formData)
      .subscribe({
        next: () => {
          this.toastr.success('Password set successfully');
          this.router.navigate(['/auth/login']);
        },
        error: err => {
          const message = err?.error?.status?.message || 'Failed to set password';
          this.toastr.error(message);
          this.isSubmitting = false;
        },
      });
  }
}
