import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormControl, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTabsModule } from '@angular/material/tabs';
import { Router, RouterLink } from '@angular/router';
import { MtxButtonModule } from '@ng-matero/extensions/button';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';

import { AuthService } from '@core/authentication';
import { MatIconModule } from '@angular/material/icon';
import { PhoneNumberValidator } from '@shared/validators/phone-number.validator';
import { MaterialModule } from '../../../../../schematics/ng-add/files/module-files/app/material.module';
import { MatPhoneFieldComponent } from '@shared/components/mat-phone-field/mat-phone-field.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MatTabsModule,
    MtxButtonModule,
    TranslateModule,
    MatIconModule,
    MaterialModule,
    MatPhoneFieldComponent,
  ],
})
export class LoginComponent implements OnInit {
  setLang(lang: string) {
    localStorage.setItem('lang', lang);
    window.location.reload();
  }

  isSubmitting = false;
  hidePassword = true;
  activeTabIndex = 0; // 0 for email, 1 for phone
  phoneApiError: string | null = null;

  emailLoginForm = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  phoneLoginForm = this.fb.nonNullable.group({
    phone: [undefined, [Validators.required]],
    password: ['', [Validators.required]],
  });

  private translate = inject(TranslateService);
  private toastr = inject(ToastrService);

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private auth: AuthService,
    private translateServices: TranslateService
  ) {}
  lang: string = '';
  ngOnInit(): void {
    this.lang = localStorage.getItem('lang') || 'en-US';
    this.translateServices.setDefaultLang('en-US');
    this.translateServices.use(this.lang || 'en-US');
  }

  get email() {
    return this.emailLoginForm.get('email')!;
  }

  get password() {
    return this.emailLoginForm.get('password')!;
  }

  get phoneControl(): FormControl {
    return this.phoneLoginForm.get('phone') as FormControl;
  }

  get phonePassword() {
    return this.phoneLoginForm.get('password')!;
  }

  togglePasswordVisibility() {
    this.hidePassword = !this.hidePassword;
  }

  // Phone input handled by mat-phone-field

  onTabChange(event: any) {
    this.activeTabIndex = event.index;
  }

  login() {
    if (this.activeTabIndex === 0) {
      this.loginWithEmail();
    } else {
      this.loginWithPhone();
    }
  }

  private loginWithEmail() {
    if (this.emailLoginForm.invalid) {
      this.markFormGroupTouched(this.emailLoginForm);
      return;
    }

    this.isSubmitting = true;

    this.auth.login(this.email.value, this.password.value).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigateByUrl('/', { replaceUrl: true });
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        console.log('err.status', err.status);
        let message = '';
        if (err && typeof err.status !== 'undefined' && err.status === 0) {
          // Network error
          message = this.lang.startsWith('ar')
            ? 'عذرًا! حدث خطأ ما. يُرجى المحاولة بعد قليل.'
            : 'Oops! Something went wrong. Please try again in a little while.';
        } else if (err?.error?.code || err?.error?.message) {
          const messageKey = err.error.code || err.error.message;
          message = this.translate.instant(messageKey);
        } else {
          message = this.lang.startsWith('ar')
            ? 'حدث خطأ غير متوقع'
            : 'An unexpected error occurred';
        }
        this.toastr.error(message);
      },
    });
  }

  private loginWithPhone() {
    if (this.phoneLoginForm.invalid) {
      this.markFormGroupTouched(this.phoneLoginForm);
      return;
    }

    this.isSubmitting = true;

    // Ensure translate uses the currently selected language
    const activeLang = (this.lang || this.translate.currentLang || 'en-US');
    this.translate.setDefaultLang(activeLang);
    this.translate.use(activeLang);

    const resolved = this.resolvePhone(this.phoneLoginForm.value.phone);
    // Reject numbers that start with 0 for specific countries (e.g., Egypt +20)
    if (
      !resolved ||
      !resolved.phoneNumber?.number ||
      !resolved.phoneNumber?.countryCode ||
      (resolved.phoneNumber.countryCode === '+20' && String(resolved.phoneNumber.number).startsWith('0'))
    ) {
      this.isSubmitting = false;
      const translated = this.translate.instant('INVALID_PHONE_OR_PASSWORD')
        || this.translate.instant('errors.invalid_phone_or_password')
        || 'Invalid phone number or password';
      const message = translated;
      this.toastr.error(message);
      return;
    }
    const dto = {
      phoneNumber: resolved?.phoneNumber?.number || '',
      countryCode: resolved?.phoneNumber?.countryCode || '',
      password: this.phonePassword.value,
    };

    this.auth.loginWithPhone(dto).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.router.navigateByUrl('/', { replaceUrl: true });
      },
      error: (err: HttpErrorResponse) => {
        this.isSubmitting = false;
        console.log('err.status', err.status);
        let message = '';
        if (err?.status === 400 || err?.status === 401) {
          // Ensure translate uses the currently selected language
          const activeLang = (this.lang || this.translate.currentLang || 'en-US');
          this.translate.setDefaultLang(activeLang);
          this.translate.use(activeLang);

          const apiMsg: string = ((err?.error?.message || err?.error?.code || '') as any).toString();
          const normalized = (apiMsg || '')
            .toLowerCase()
            .replace(/[\.,!]/g, ' ')
            .replace(/\s+/g, ' ')
            .trim();
          let key = '';
          if (
            normalized.includes('Invalid Phone number or Password.') 
          ) {
            key = 'INVALID_PHONE_OR_PASSWORD';
          } else if (normalized.includes('invalid email or password')) {
            key = 'INVALID_EMAIL_OR_PASSWORD';
          } else {
            key = (err?.error?.code || '').toString();
          }

          let translated = '';
          if (key) {
            translated = this.translate.instant(key) || '';
          }
          if (!translated || translated === key) {
            translated = this.translate.instant('INVALID_PHONE_OR_PASSWORD')
              || this.translate.instant('errors.invalid_phone_or_password');
          }
          message = translated && typeof translated === 'string' && translated.length > 0
            ? translated
            : 'Invalid phone number or password';
        } else if (err && typeof err.status !== 'undefined' && err.status === 0) {
          // Network error
          message = this.lang.startsWith('ar')
            ? 'عذرًا! حدث خطأ ما. يُرجى المحاولة بعد قليل.'
            : 'Oops! Something went wrong. Please try again in a little while.';
        } else if (err?.error?.code || err?.error?.message) {
          // For phone login, always use the specific translation key if present
          const messageKey = err.error.code || err.error.message || 'INVALID_PHONE_OR_PASSWORD';
          message = this.translate.instant(messageKey);
        } else {
          message = this.lang.startsWith('ar')
            ? 'حدث خطأ غير متوقع'
            : 'An unexpected error occurred';
        }
        this.toastr.error(message);
      },
    });
  }

  private markFormGroupTouched(formGroup: any) {
    Object.keys(formGroup.controls).forEach(key => {
      const control = formGroup.get(key);
      control?.markAsTouched();
    });
  }

  private resolvePhone(phoneValue: any): { phoneNumber: { number: string; countryCode: string } } | null {
    if (!phoneValue || !phoneValue.number || !phoneValue.dialCode) {
      return null;
    }

    const digitsOnly = String(phoneValue.number).replace(/\s/g, '');

    return {
      phoneNumber: {
        number: digitsOnly,
        countryCode: phoneValue.dialCode,
      },
    };
  }

}
