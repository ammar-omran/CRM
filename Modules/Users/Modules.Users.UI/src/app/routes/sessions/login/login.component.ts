import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { MtxButtonModule } from '@ng-matero/extensions/button';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { filter, finalize, tap } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';

import { AuthService } from '@core/authentication';
import { MatIconModule } from '@angular/material/icon';
import { LanguageService } from '@shared/services/language.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
    MatButtonModule,
    MatCardModule,
    MatCheckboxModule,
    MatFormFieldModule,
    MatInputModule,
    MtxButtonModule,
    TranslateModule,
    MatIconModule,
  ],
})
export class LoginComponent implements OnInit {
  isSubmitting = false;
  currentLanguage = 'en-US';

  loginForm = this.fb.nonNullable.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]],
  });

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private auth: AuthService,
    private toastr: ToastrService,
    private translateService: TranslateService,
    private languageService: LanguageService
  ) { }

  /**
   * Initializes the component and sets up language handling
   * Subscribes to language changes and sets the initial language
   */
  ngOnInit(): void {
    // Subscribe to language changes from the service
    this.languageService.currentLanguage$.subscribe(lang => {
      this.currentLanguage = lang;
      // Set document direction based on language
      const direction = lang === 'ar-EG' ? 'rtl' : 'ltr';
      document.documentElement.setAttribute('dir', direction);
      document.body.setAttribute('dir', direction);
    });

    // Set initial language from the service
    this.currentLanguage = this.languageService.getCurrentLanguage();
    const initialDirection = this.currentLanguage === 'ar-EG' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', initialDirection);
    document.body.setAttribute('dir', initialDirection);

    // If already authenticated, skip login and go to dashboard
    if (this.auth.check()) {
      this.router.navigate(['/dashboard'], { replaceUrl: true });
    }
  }

  /**
   * Getter for the username form control
   * @returns The username form control instance
   */
  get username() {
    return this.loginForm.get('username')!;
  }

  /**
   * Getter for the password form control
   * @returns The password form control instance
   */
  get password() {
    return this.loginForm.get('password')!;
  }

  /**
   * Toggles the application language between English and Arabic
   * Updates the UI and persists the language preference
   */
  toggleLanguage() {
    const currentLang = this.languageService.getCurrentLanguage();
    const newLang = currentLang === 'en-US' ? 'ar-EG' : 'en-US';
    this.languageService.setLanguage(newLang);
    this.currentLanguage = newLang;

    // Update document direction
    const direction = newLang === 'ar-EG' ? 'rtl' : 'ltr';
    document.documentElement.setAttribute('dir', direction);
    document.body.setAttribute('dir', direction);
  }

  // Test method to verify translations
  testTranslations() {
    console.log('=== Translation Test ===');
    console.log('Current language:', this.currentLanguage);
    console.log('Available languages:', this.translateService.getLangs());
    console.log('Default language:', this.translateService.getDefaultLang());

    const testKeys = ['login_title', 'username', 'password', 'invalid_credentials'];
    testKeys.forEach(key => {
      const translation = this.translateService.instant(key);
      console.log(`${key}: ${translation}`);
    });
  }

  /**
   * Retrieves a translation for the given key with fallback support
   * @param key The translation key to look up
   * @param fallback The default text to use if translation is not found
   * @returns The translated text or fallback if not available
   */
  getTranslation(key: string, fallback: string): string {
    const translation = this.translateService.instant(key);
    return translation !== key ? translation : fallback;
  }

  /**
   * Handles the login form submission
   * Validates the form, authenticates the user, and navigates to dashboard on success
   */
  login() {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      this.showValidationErrors();
      return;
    }

    this.isSubmitting = true;

    this.auth
      .login(this.username.value, this.password.value)
      .pipe(
        tap(result => {
          // Process authentication result
        }),
        filter(authenticated => {
          // Only proceed if authentication is successful
          return authenticated;
        }),
        finalize(() => {
          // Reset submission state regardless of outcome
          this.isSubmitting = false;
        })
      )
      .subscribe({
        next: result => {
          // Display success message and redirect to dashboard
          this.toastr.success(
            this.translateService.instant('login_success'),
            this.translateService.instant('login_title')
          );
          this.router.navigate(['/dashboard'], { replaceUrl: true });
        },
        error: (errorRes: HttpErrorResponse) => {
          // Handle authentication errors
          this.handleLoginError(errorRes);
        },
      });
  }

  /**
   * Processes HTTP error responses from the authentication service
   * Maps error status codes to appropriate user messages and form validations
   * @param errorRes HTTP error response from the authentication attempt
   */
  private handleLoginError(errorRes: HttpErrorResponse) {
    let errorMessage = '';

    if (errorRes.status === 422) {
      // Process validation errors from server and apply to form controls
      const form = this.loginForm;
      const errors = errorRes.error ?.errors;
      if (errors) {
        Object.keys(errors).forEach(key => {
          const fieldName = key === 'email' ? 'username' : key;
          const control = form.get(fieldName);
          if (control) {
            control.setErrors({
              remote: errors[key][0],
            });
          }
        });
        errorMessage = this.translateService.instant('validations.required');
      }
    } else if (errorRes.status === 401) {
      errorMessage = this.translateService.instant('invalid_credentials');
    } else if (errorRes.status === 403) {
      errorMessage = this.translateService.instant('account_locked');
    } else if (errorRes.status === 0) {
      errorMessage = this.translateService.instant('network_error');
    } else if (errorRes.error ?.message) {
      errorMessage = errorRes.error.message;
    } else {
      errorMessage = this.translateService.instant('server_error');
    }

    // Display error notification to user
    this.toastr.error(errorMessage, this.translateService.instant('login_error'), {
      timeOut: 5000,
      extendedTimeOut: 2000,
      closeButton: true,
      progressBar: true,
      positionClass: 'toast-top-right',
    });
  }

  /**
   * Collects and displays validation errors from the login form
   * Generates user-friendly error messages for required fields
   */
  private showValidationErrors() {
    const errors: string[] = [];

    if (this.username.hasError('required')) {
      const fieldName = this.translateService.instant('username');
      errors.push(this.translateService.instant('validations.required_field', { fieldName }));
    }

    if (this.password.hasError('required')) {
      const fieldName = this.translateService.instant('password');
      errors.push(this.translateService.instant('validations.required_field', { fieldName }));
    }

    if (errors.length > 0) {
      const errorMessage = errors.join('\n');
      this.toastr.error(errorMessage, this.translateService.instant('login_error'), {
        timeOut: 5000,
        extendedTimeOut: 2000,
        closeButton: true,
        progressBar: true,
        positionClass: 'toast-top-right',
      });
    }
  }

  /**
   * Marks all form controls as touched to trigger validation visuals
   * Ensures validation errors are displayed to the user
   */
  private markFormGroupTouched() {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control ?.markAsTouched();
    });
  }
}
