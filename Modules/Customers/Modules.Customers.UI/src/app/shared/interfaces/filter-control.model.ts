import { ValidatorFn } from '@angular/forms';
import { EndPoint } from '@shared/enums';

export interface FilterControl {
  formControlName: string;
  label: string;
  type: 'text' | 'select' | 'autocomplete' | 'date' | 'checkbox' | 'radio';
  options?: Array<{ value: any; label: string }>;
  validators?: Array<ValidatorFn>;
  apiEndpoint?: EndPoint;
  key?: string; //if the data for example not coming as an array of string like array of objects , we have to know each value then
  isFirstValueDynamic?: boolean;
  optionVal?: string;
  optionLabel?: string;
  initialValue?: any;
}
