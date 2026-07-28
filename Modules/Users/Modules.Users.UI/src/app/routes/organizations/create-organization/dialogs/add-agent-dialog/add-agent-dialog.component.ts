import { CommonModule } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { BaseResponse } from '@shared/interfaces/base-response';
import { InfoBannerComponent } from '@shared/components/info-banner/info-banner.component';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

//  Regex: 8–12 chars, at least one digit, at least one special char
const PASSWORD_PATTERN = /^(?=.*[0-9])(?=.*[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]).{8,12}$/;

//  Regex: letters and single spaces only, no leading/trailing spaces
const NAME_PATTERN = /^[a-zA-Z\u0600-\u06FF]+( [a-zA-Z\u0600-\u06FF]+)*$/;

//  Regex: digits only
const PHONE_PATTERN = /^[0-9]+$/;

/** Role options shared with create-organization inline lookup. */
export const ORGANIZATION_AGENT_ROLE_OPTIONS: ReadonlyArray<{
  value: string;
  labelKey: string;
}> = [
  { value: '1', labelKey: 'organizations.agent_role_first_line' },
  { value: '2', labelKey: 'organizations.agent_role_second_line' },
  { value: '3', labelKey: 'organizations.agent_role_team_leader' },
];

export interface AddAgentDialogData {
  prefillEmail?: string;
  presetRole?: string;
  /** Use shorter “New Agent” title (e.g. when opened after email lookup). */
  useNewAgentTitle?: boolean;
}

@Component({
  selector: 'app-add-agent-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatIconModule,
    TranslateModule,
    InfoBannerComponent,
  ],
  templateUrl: './add-agent-dialog.component.html',
  styleUrl: './add-agent-dialog.component.scss',
})
export class AddAgentDialogComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject<MatDialogRef<AddAgentDialogComponent>>(MatDialogRef);
  private apiService = inject(ApiService);
  private translateService = inject(TranslateService);
  readonly dialogData = inject<AddAgentDialogData | null>(MAT_DIALOG_DATA, { optional: true });

  form: FormGroup = this.fb.group({
    name: ['', [Validators.required, Validators.pattern(NAME_PATTERN)]],
    email: ['', [Validators.required, Validators.email]],
    phone: ['', [Validators.required, Validators.pattern(PHONE_PATTERN)]],
    role: ['', [Validators.required]],
    password: ['', [Validators.required, Validators.pattern(PASSWORD_PATTERN)]],
  });

  readonly roles = ORGANIZATION_AGENT_ROLE_OPTIONS;

  showPassword = false;
  isSaving = false;

  /**
   * When the entered email already exists in the system, this holds the
   * matching agent so the template can show the confirmation banner.
   */
  duplicateAgent: OranizationAgent | null = null;

  ngOnInit(): void {
    const d = this.dialogData;
    if (d?.prefillEmail) {
      this.form.patchValue({ email: d.prefillEmail.trim() });
    }
    if (d?.presetRole) {
      this.form.patchValue({ role: d.presetRole });
    }
  }

  /** Resolves a translation key to its string value — used inside template
   *  pipe params where nested pipes are not supported. */
  fieldLabel(key: string): string {
    return this.translateService.instant(key);
  }

  save(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    const { role, ...payload } = this.form.getRawValue();

    this.apiService
      .triggerApiRequest<
        BaseResponse<OranizationAgent>
      >(EndPoint.ADD_AGENT, HttpVerb.POST, null, payload)
      .subscribe({
        next: response => {
          // Created: close dialog
          response.data.role = role;
          this.dialogRef.close(response.data);
        },
        error: (response: BaseResponse<OranizationAgent>) => {
          if (response.status.code === ResponseStatusEnum.Conflict) {
            // Duplicate: show banner, let user decide
            this.duplicateAgent = response.data;
            this.duplicateAgent.role = role;
          }
          this.isSaving = false;
        },
        complete: () => {
          this.isSaving = false;
        },
      });
  }

  /** User confirmed they want to add the existing agent to the organisation. */
  confirmAddExisting(): void {
    if (!this.duplicateAgent) return;
    this.dialogRef.close(this.duplicateAgent);
  }

  dismissDuplicate(): void {
    this.duplicateAgent = null;
  }

  cancel(): void {
    this.dialogRef.close();
  }
}

export type OranizationAgent = {
  id: string;
  email: string;
  name: string;
  phone: string;
  role: string;
};
