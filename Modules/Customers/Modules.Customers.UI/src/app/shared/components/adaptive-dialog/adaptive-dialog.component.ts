import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import {
  BaseDialogData,
  ConfirmationDialogData,
  FormDialogData,
} from '@shared/interfaces/dialog-data.model';

@Component({
  selector: 'app-adaptive-dialog',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatSelectModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
  ],
  templateUrl: './adaptive-dialog.component.html',
  styleUrl: './adaptive-dialog.component.scss',
})
export class AdaptiveDialogComponent {
  form: FormGroup;

  constructor(
    public dialogRef: MatDialogRef<AdaptiveDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: FormDialogData | ConfirmationDialogData,
    private fb: FormBuilder
  ) {
    this.form = this.fb.group({});
    if (this.isFormDialog(data)) {
      this.initForm(data);
    }
  }

  private isFormDialog(data: BaseDialogData): data is FormDialogData {
    return data.mode === 'form';
  }

  private initForm(data: FormDialogData): void {
    data.fields.forEach(field => {
      this.form.addControl(field.formControlName, this.fb.control(null, Validators.required));
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  onConfirm(): void {
    if (this.isFormDialog(this.data)) {
      if (this.form.valid) {
        this.dialogRef.close(this.form.value);
      }
    } else {
      this.dialogRef.close(true);
    }
  }
}
