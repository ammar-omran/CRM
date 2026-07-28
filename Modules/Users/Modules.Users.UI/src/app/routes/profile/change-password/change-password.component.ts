import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { TokenService } from '@core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CustomValidators } from '@shared/utils/custom-validators';
import { Router } from '@angular/router';
import { NgClass } from '@angular/common';

@Component({
  selector: 'app-change-password',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslateModule,
    MatInputModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    NgClass,
  ],
  templateUrl: './change-password.component.html',
  styleUrl: './change-password.component.scss',
})
export class ChangePasswordComponent implements OnInit {
  changePasswordForm!: FormGroup;
  private tokenService = inject(TokenService);
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  passwordCriteria = {
    minLength: false,
    uppercase: false,
    lowercase: false,
    specialCharacter: false,
  };
  constructor() {}

  ngOnInit(): void {
    this.initializeForm();
  }
  private initializeForm(): void {
    this.changePasswordForm = this.fb.group({
      oldPassword: ['', Validators.required],
      newPassword: ['', [Validators.required, CustomValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required, CustomValidators.confirmPassword('newPassword')]],
    });

    this.changePasswordForm.get('newPassword')?.valueChanges.subscribe(() => {
      this.updatePasswordChecklist();
    });
  }
  updatePasswordChecklist(): void {
    const password = this.changePasswordForm.get('newPassword')?.value || '';
    this.passwordCriteria.minLength = password.length >= 8;
    this.passwordCriteria.uppercase = /[A-Z]/.test(password);
    this.passwordCriteria.lowercase = /[a-z]/.test(password);
    this.passwordCriteria.specialCharacter = /[\W_]/.test(password);
  }

  allCriteriaMet(): boolean {
    return Object.values(this.passwordCriteria).every(value => value === true);
  }
  onSubmit(): void {
    if (this.changePasswordForm.invalid) return;

    const userName = this.tokenService.getUsername();
    if (!userName) {
      this.toastr.error('User not authenticated.');
      return;
    }

    const formData = {
      userName,
      oldPassword: this.changePasswordForm.value.oldPassword,
      newPassword: this.changePasswordForm.value.newPassword,
    };

    this.apiService
      .triggerApiRequest(EndPoint.CHANGE_PASSWORD, HttpVerb.PUT, null, formData)
      .subscribe({
        next: () => {
          this.toastr.success('Password changed successfully');
          this.router.navigate(['/dashboard']);
        },
      });
  }
}
