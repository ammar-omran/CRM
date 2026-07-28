import { AbstractControl, ValidationErrors } from '@angular/forms';

export class PhoneNumberValidator {
  static validatePhoneNumber(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null; // Let required validator handle empty values
    }

    const phoneNumber = control.value.toString().trim();
    
    // Check if contains non-digit characters
    if (!/^\d+$/.test(phoneNumber)) {
      return { invalidPhoneFormat: true };
    }
    
    // Check length (between 10 and 15 digits)
    if (phoneNumber.length < 10 || phoneNumber.length > 15) {
      return { invalidPhoneLength: true };
    }
    
    return null;
  }
} 