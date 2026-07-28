import { AbstractControl } from '@angular/forms';
import { PhoneNumberValidator } from './phone-number.validator';

describe('PhoneNumberValidator', () => {
  let control: AbstractControl;

  beforeEach(() => {
    control = {
      value: '',
      errors: null,
      setErrors: (errors: any) => {
        (control as any).errors = errors;
      }
    } as AbstractControl;
  });

  it('should return null for empty value', () => {
    (control as any).value = '';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toBeNull();
  });

  it('should return null for valid 10-digit phone number', () => {
    (control as any).value = '1234567890';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toBeNull();
  });

  it('should return null for valid 15-digit phone number', () => {
    (control as any).value = '123456789012345';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toBeNull();
  });

  it('should return invalidPhoneFormat for phone number with letters', () => {
    (control as any).value = '123456789a';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toEqual({ invalidPhoneFormat: true });
  });

  it('should return invalidPhoneFormat for phone number with special characters', () => {
    (control as any).value = '123-456-7890';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toEqual({ invalidPhoneFormat: true });
  });

  it('should return invalidPhoneLength for phone number with less than 10 digits', () => {
    (control as any).value = '123456789';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toEqual({ invalidPhoneLength: true });
  });

  it('should return invalidPhoneLength for phone number with more than 15 digits', () => {
    (control as any).value = '1234567890123456';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toEqual({ invalidPhoneLength: true });
  });

  it('should trim whitespace before validation', () => {
    (control as any).value = ' 1234567890 ';
    const result = PhoneNumberValidator.validatePhoneNumber(control);
    expect(result).toBeNull();
  });
}); 