import { AbstractControl, ValidationErrors } from '@angular/forms';

export class StringValidator {
  static isEmptyString(control: AbstractControl): ValidationErrors | null {
    if (control.value && typeof control.value === 'string' && control.value.trim() === '') {
      return { isEmptyString: true };
    }
    return null;
  }
}
