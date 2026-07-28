import { Component, Input } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { CountryISO, SearchCountryField, PhoneNumberFormat } from 'ngx-intl-tel-input';
import { NgxIntlTelInputModule } from 'ngx-intl-tel-input';
import { ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'mat-phone-field',
  templateUrl: './mat-phone-field.component.html',
  styleUrls: ['./mat-phone-field.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    NgxIntlTelInputModule,
    ReactiveFormsModule,
    TranslateModule,
    MatInputModule,
    MatFormFieldModule,
    MatIconModule,
  ],
})
export class MatPhoneFieldComponent {
  @Input() control!: FormControl;

  SearchCountryField = SearchCountryField;
  CountryISO = CountryISO;
  PhoneNumberFormat = PhoneNumberFormat;
}
