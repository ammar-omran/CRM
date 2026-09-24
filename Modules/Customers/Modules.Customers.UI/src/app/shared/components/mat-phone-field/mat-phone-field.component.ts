import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe } from '@ngx-translate/core';
import { Subscription } from 'rxjs';

export interface CountryOption {
  iso2: 'eg' | 'us';
  name: string;
  dialCode: string;
  flag: string;
  placeholder: string;
  minLength: number;
  maxLength: number;
}

@Component({
  selector: 'mat-phone-field',
  templateUrl: './mat-phone-field.component.html',
  styleUrls: ['./mat-phone-field.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatIconModule,
    TranslatePipe,
  ],
})
export class MatPhoneFieldComponent implements OnInit, OnDestroy {
  @Input() control!: FormControl;

  countries: CountryOption[] = [
    {
      iso2: 'eg',
      name: 'Egypt',
      dialCode: '+20',
      flag: '🇪🇬',
      placeholder: '10xxxxxxxx',
      minLength: 10,
      maxLength: 11,
    },
    {
      iso2: 'us',
      name: 'United States',
      dialCode: '+1',
      flag: '🇺🇸',
      placeholder: '(201) 555-0123',
      minLength: 10,
      maxLength: 10,
    },
  ];

  selectedCountry: CountryOption = this.countries[0];
  phoneNumber = '';
  private sub?: Subscription;

  ngOnInit(): void {
    // Initialize from existing control value (for edit mode)
    const val = this.control?.value;
    if (val && typeof val === 'object' && val.dialCode) {
      const found = this.countries.find(c => c.dialCode === val.dialCode);
      if (found) this.selectedCountry = found;
      this.phoneNumber = val.number ?? val.phoneNumber ?? '';
    } else if (typeof val === 'string' && val) {
      // Handle string value like "+201234567890" or "01234567890"
      this.phoneNumber = val.replace(/\D/g, '').replace(/^20|^1/, '');
    }

    // Keep control in sync when user types
    this.sub = this.control?.valueChanges.subscribe(() => {
      // Avoid loop - just ensure phoneNumber reflects control if externally patched
    });
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }

  onCountryChange(country: CountryOption): void {
    this.selectedCountry = country;
    this.updateControl();
  }

  onPhoneInput(value: string): void {
    // Only allow digits, limit to maxLength
    const digits = value.replace(/\D/g, '').slice(0, this.selectedCountry.maxLength);
    this.phoneNumber = digits;
    this.updateControl();
  }

  private updateControl(): void {
    const digits = this.phoneNumber.replace(/\D/g, '');
    if (!digits) {
      this.control.setValue(null, { emitEvent: false });
      this.control.markAsTouched();
      return;
    }

    // Build value shape expected by existing resolvePhone() in login/register:
    // { number: string, dialCode: string, countryCode: string, iso2: string }
    const value = {
      number: digits,
      dialCode: this.selectedCountry.dialCode,
      countryCode: this.selectedCountry.dialCode,
      iso2: this.selectedCountry.iso2,
      dialCodePlaceholder: this.selectedCountry.placeholder,
    };
    this.control.setValue(value as any, { emitEvent: true });
    this.control.markAsDirty();
    this.control.markAsTouched();
    this.control.updateValueAndValidity({ emitEvent: false });
  }

  getErrorMessage(): string {
    if (this.control.hasError('required')) return 'phone_required';
    return 'invalid_phone_format';
  }
}
