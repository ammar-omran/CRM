import { Component, inject } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { CustomValidators } from '@shared/utils/custom-validators';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { BaseResponse } from '@shared/interfaces/base-response';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';
import { MatPhoneFieldComponent } from '@shared/components/mat-phone-field/mat-phone-field.component';
import { ValidationProblemDetails } from '@shared/interfaces/validation-problem-details';
import { MtxButtonModule } from '@ng-matero/extensions/button';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  standalone: true,
  imports: [
    RouterLink,
    FormsModule,
    ReactiveFormsModule,
    MatButtonModule,
    MtxButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    TranslateModule,
    MatPhoneFieldComponent,
  ],
})
export class RegisterComponent {
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private router = inject(Router);
  private translate = inject(TranslateService);

  constructor(private fb: FormBuilder) { }

  registerForm = this.fb.nonNullable.group(
    {
      name: ['', [Validators.required, Validators.pattern(/^[\p{L}]+(?: [\p{L}]+)+$/u)]],
      phone: [undefined, [Validators.required]],
      email: [
        '',
        [
          Validators.required,
          Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/),
        ],
      ],
      password: ['', [Validators.required, CustomValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required]],
    },
    {
      validators: [CustomValidators.matchValidator('password', 'confirmPassword')],
    }
  );

  isSubmitting = false;

  onSubmit() {
    if (this.registerForm.invalid) return;
    this.isSubmitting = true;

    const resolvedPhone = this.resolvePhone(this.registerForm.value.phone);
    const requestData = {
      name: this.registerForm.value.name,
      email: this.registerForm.value.email,
      phoneNumber: resolvedPhone ? resolvedPhone.phoneNumber : { number: '', countryCode: '' },
      password: this.registerForm.value.password,
    };

    this.apiService
      .triggerApiRequest<BaseResponse<any>>(
        EndPoint.REGISTER,
        HttpVerb.POST,
        null,
        requestData
      )
      .subscribe({
        next: response => {

          if (response.status.code === ResponseStatusEnum.Success) {
            this.toastr.success(this.translate.instant('register_success'), '');

            const { maskedEmail, hashedEmail } = response.data;
            this.router.navigate(
              ['/auth/email-confirmation'],
              { queryParams: { token: hashedEmail, maskedEmail } }
            );
          } else {

            const translationKey = this.resolveErrorKey(response.status.message);
            const localizedMessage = this.translate.instant(translationKey);
            this.toastr.error(localizedMessage, '');
          }
        },
        error: err => {

          if (this.isValidationError(err.error)) {
            const validationErrors = err.error.errors;
            for (const field in validationErrors) {
              if (validationErrors.hasOwnProperty(field)) {
                validationErrors[field].forEach((msg: string) => {
                  this.toastr.error(`${field}: ${msg}`, 'Validation Error');
                });
              }
            }
          } else if (err.message) {
            this.toastr.error(err.message, 'Error');
          } else {
            this.toastr.error('An unexpected error occurred.', 'Error');
          }
          this.isSubmitting = false;
        },
        complete: () => {
          this.isSubmitting = false;
        },
      });
  }


  private resolveErrorKey(backendMessage: string): string {
    const normalized = (backendMessage ?? '').toLowerCase().trim();

    if (normalized.includes('email') && normalized.includes('used')) {
      return 'register.email_already_used';
    }

    if (
      (normalized.includes('phone') || normalized.includes('number')) &&
      normalized.includes('used')
    ) {
      return 'register.phone_already_used';
    }

    // Return the raw backend message as a final fallback so something
    // meaningful is always shown to the user.
    return backendMessage ?? 'register_error_generic';
  }

  private isValidationError(error: any): error is ValidationProblemDetails {
    return (
      error && typeof error === 'object' && 'errors' in error && typeof error.errors === 'object'
    );
  }

  get phoneControl(): FormControl {
    return this.registerForm.get('phone') as FormControl;
  }

  resolvePhone(phoneValue: any): { phoneNumber: { number: string; countryCode: string } } | null {
    if (!phoneValue || !phoneValue.number || !phoneValue.dialCode) {
      return null;
    }

    const digitsOnly = phoneValue.number.replace(' ', '');

    return {
      phoneNumber: {
        number: digitsOnly,
        countryCode: phoneValue.dialCode,
      },
    };
  }
}
