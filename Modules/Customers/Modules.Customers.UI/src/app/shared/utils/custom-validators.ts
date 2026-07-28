import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export class CustomValidators {
  static confirmPassword(passwordControlName: string): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const confirmPassword = control.value;
      const passwordControl = control.parent?.get(passwordControlName);

      if (!passwordControl) {
        return null;
      }
      const password = passwordControl.value;
      return password !== confirmPassword ? { confirmPassword: true } : null;
    };
  }
  static matchValidator(source: string, target: string) {
    return (control: AbstractControl) => {
      const sourceControl = control.get(source)!;
      const targetControl = control.get(target)!;
      if (targetControl.errors && !targetControl.errors.mismatch) {
        return null;
      }
      if (sourceControl.value !== targetControl.value) {
        targetControl.setErrors({ mismatch: true });
        return { mismatch: true };
      } else {
        targetControl.setErrors(null);
        return null;
      }
    };
  }

  static strongPassword(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      const password = control.value;
      if (!password) return null;

      const errors: ValidationErrors = {};

      const lengthRegex = /.{8,}/; // at least 8 characters
      const uppercaseRegex = /[A-Z]/; // at least one uppercase letter
      const lowercaseRegex = /[a-z]/; // at least one lowercase letter
      const specialCharRegex = /[\W_]/; // at least one special character

      if (!lengthRegex.test(password)) {
        errors.minLength = true;
      }
      if (!uppercaseRegex.test(password)) {
        errors.uppercase = true;
      }
      if (!lowercaseRegex.test(password)) {
        errors.lowercase = true;
      }
      if (!specialCharRegex.test(password)) {
        errors.specialCharacter = true;
      }

      return Object.keys(errors).length ? errors : null;
    };
  }
}
