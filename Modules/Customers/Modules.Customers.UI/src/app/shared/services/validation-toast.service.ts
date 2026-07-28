import { Injectable } from '@angular/core';
import { AbstractControl, FormGroup } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';

@Injectable({
  providedIn: 'root'
})
export class ValidationToastService {

  constructor(
    private toastr: ToastrService,
    private translate: TranslateService
  ) {}

  /**
   * Display validation errors for a form control as toast notifications
   */
  showControlErrors(control: AbstractControl, fieldName: string): void {
    if (control && control.errors && control.touched) {
      const errors = control.errors;
      
      // Handle required error
      if (errors['required']) {
        this.showError('field_required', { fieldName: this.translate.instant(fieldName) });
      }
      
      // Handle email errors
      if (errors['email'] || errors['invalidEmailFormat']) {
        this.showError('invalid_email_format');
      }
      
      // Handle minlength error
      if (errors['minlength']) {
        this.showError('minlength', { number: errors['minlength'].requiredLength });
      }
      
      // Handle maxlength error
      if (errors['maxlength']) {
        this.showError('maxlength', { number: errors['maxlength'].requiredLength });
      }
      
      // Handle min error
      if (errors['min']) {
        this.showError('min', { number: errors['min'].min });
      }
      
      // Handle max error
      if (errors['max']) {
        this.showError('max', { number: errors['max'].max });
      }
      
      // Handle custom validation errors
      if (errors['invalidCredentials']) {
        this.showError('invalid_credentials');
      }
      
      if (errors['networkError']) {
        this.showError('network_error');
      }
      
      if (errors['serverError']) {
        this.showError('server_error');
      }
      
      if (errors['passwordMismatch']) {
        this.showError('passwords_do_not_match');
      }
    }
  }

  /**
   * Display validation errors for all controls in a form
   */
  showFormErrors(form: FormGroup): void {
    Object.keys(form.controls).forEach(key => {
      const control = form.get(key);
      if (control) {
        this.showControlErrors(control, key);
        
        // Handle nested form groups
        if (control instanceof FormGroup) {
          this.showFormErrors(control);
        }
      }
    });
  }

  /**
   * Show error toast with translation
   */
  private showError(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.toastr.error(message, '', {
      timeOut: 5000,
      positionClass: 'toast-top-right',
      enableHtml: true
    });
  }

  /**
   * Show success toast with translation
   */
  showSuccess(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.toastr.success(message, '', {
      timeOut: 3000,
      positionClass: 'toast-top-right',
      enableHtml: true
    });
  }

  /**
   * Show info toast with translation
   */
  showInfo(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.toastr.info(message, '', {
      timeOut: 4000,
      positionClass: 'toast-top-right',
      enableHtml: true
    });
  }

  /**
   * Show warning toast with translation
   */
  showWarning(messageKey: string, params?: any): void {
    const message = this.translate.instant(messageKey, params);
    this.toastr.warning(message, '', {
      timeOut: 4000,
      positionClass: 'toast-top-right',
      enableHtml: true
    });
  }
}