export interface BaseDialogData {
  title: string;
  mode: 'confirmation' | 'form';
}
export interface ConfirmationDialogData extends BaseDialogData {
  mode: 'confirmation';
  message: string;
}
export interface FormDialogData extends BaseDialogData {
  mode: 'form';
  fields: {
    label: string;
    formControlName: string;
    type: string;
    options?: any[];
  }[];
}
