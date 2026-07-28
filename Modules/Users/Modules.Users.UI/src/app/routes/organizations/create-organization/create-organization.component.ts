import { HttpErrorResponse } from '@angular/common/http';
import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { StringValidator } from '@shared/validators/is-empty-string';
import { Subscription, finalize } from 'rxjs';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import {
  AddAgentDialogComponent,
  OranizationAgent,
  ORGANIZATION_AGENT_ROLE_OPTIONS,
} from './dialogs/add-agent-dialog/add-agent-dialog.component';
import { AssignCustomerDialogComponent } from './dialogs/assign-customer-dialog/assign-customer-dialog.component';
import { BaseResponse } from '@shared/interfaces/base-response';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

@Component({
  selector: 'app-create-organization',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatDialogModule,
    TranslateModule,
    PageHeaderComponent,
  ],
  templateUrl: './create-organization.component.html',
  styleUrl: './create-organization.component.scss',
})
export class CreateOrganizationComponent implements OnInit, OnDestroy {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private dialog = inject(MatDialog);
  private router = inject(Router);
  private translateService = inject(TranslateService);

  organizationForm!: FormGroup;
  agentLookupForm!: FormGroup;
  readonly agentRoleOptions = ORGANIZATION_AGENT_ROLE_OPTIONS;
  isLookingUpAgent = false;
  private subscriptions: Subscription[] = [];

  ngOnInit(): void {
    this.organizationForm = this.fb.group({
      name: ['', [Validators.required, StringValidator.isEmptyString]],
      agents: this.fb.array([], [Validators.minLength(1)]),
      customers: this.fb.array([], [Validators.minLength(1)]),
    });
    this.agentLookupForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      role: ['', [Validators.required]],
    });
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(s => s.unsubscribe());
  }

  get agents(): FormArray {
    return this.organizationForm.get('agents') as FormArray;
  }

  get customers(): FormArray {
    return this.organizationForm.get('customers') as FormArray;
  }

  get isAgentLookupMode(): boolean {
    const email = (this.agentLookupForm.get('email')?.value as string)?.trim();
    const role = this.agentLookupForm.get('role')?.value;
    return !!email && !!role;
  }

  addAgent(): void {
    if (this.isAgentLookupMode) {
      this.lookupAgentByEmailAndRole();
      return;
    }

    const dialogRef = this.dialog.open(AddAgentDialogComponent, {
      width: '520px',
      disableClose: true,
    });

    const sub = dialogRef.afterClosed().subscribe((agent: OranizationAgent | undefined) => {
      if (agent) {
        this.addAgentToOrganizationForm(agent);
      }
    });

    this.subscriptions.push(sub);
  }

  /**
   * GET `agents` with `email` and `role` query params. On success adds the agent;
   * on not-found opens the new-agent dialog with email and role preset.
   */
  lookupAgentByEmailAndRole(): void {
    if (this.agentLookupForm.invalid) {
      this.agentLookupForm.markAllAsTouched();
      return;
    }

    const email = (this.agentLookupForm.value.email as string).trim();
    const role = this.agentLookupForm.value.role as string;

    this.isLookingUpAgent = true;
    this.apiService
      .triggerApiRequest<{ items: OranizationAgent[]; totalItemsCount: number }>(
        EndPoint.AGENTS_LIST,
        HttpVerb.GET,
        { email, role }
      )
      .pipe(finalize(() => (this.isLookingUpAgent = false)))
      .subscribe({
        next: response => {
          if (response.totalItemsCount > 0 && response.items && response.items.length > 0) {
            const agent = response.items[0];
            agent.role = role;
            this.addAgentToOrganizationForm(agent);
            this.agentLookupForm.reset({ email: '', role: '' });
            return;
          }
          if (response.totalItemsCount == 0) {
            this.openNewAgentDialogForEmail(email, role);
            return;
          }
        },
        error: (err: unknown) => {
          if (err instanceof HttpErrorResponse && err.status === 404) {
            this.openNewAgentDialogForEmail(email, role);
            return;
          }
          this.toastr.error(this.translateService.instant('organizations.agent_lookup_error'));
        },
      });
  }

  private openNewAgentDialogForEmail(email: string, role: string): void {
    const dialogRef = this.dialog.open(AddAgentDialogComponent, {
      width: '520px',
      disableClose: true,
      data: {
        prefillEmail: email,
        presetRole: role,
        useNewAgentTitle: true,
      },
    });

    const sub = dialogRef.afterClosed().subscribe((agent: OranizationAgent | undefined) => {
      if (agent) {
        this.addAgentToOrganizationForm(agent);
        this.agentLookupForm.reset({ email: '', role: '' });
      }
    });
    this.subscriptions.push(sub);
  }

  private addAgentToOrganizationForm(agent: OranizationAgent): void {
    const hasAgent = this.agents.controls.some(
      ctrl => ctrl.value.id === agent.id && ctrl.value.role === agent.role
    );

    if (hasAgent) {
      return;
    }

    const hasAnotherRole = this.agents.controls.some(
      ctrl => ctrl.value.id === agent.id && ctrl.value.role !== agent.role
    );

    if (hasAnotherRole) {
      this.toastr.error(this.translateService.instant('organizations.error_agent_one_role'));
      return;
    }

    const hasLeader = this.agents.controls.some(ctrl => ctrl.value.role == '3');

    if (agent.role == '3' && hasLeader) {
      this.toastr.error(this.translateService.instant('organizations.error_one_leader'));
      return;
    }

    this.agents.push(
      this.fb.group({
        id: [agent.id],
        name: [agent.name],
        email: [agent.email],
        phone: [agent.phone],
        role: [agent.role],
      })
    );
  }

  assignCustomer(): void {
    const dialogRef = this.dialog.open(AssignCustomerDialogComponent, {
      width: '420px',
      disableClose: true,
    });

    const sub = dialogRef.afterClosed().subscribe((result: any[] | undefined) => {
      if (result && Array.isArray(result)) {
        result.forEach(customer => {
          const exists = this.customers.controls.some(ctrl => ctrl.value.id === customer.id);
          if (!exists) {
            this.customers.push(
              this.fb.group({
                id: [customer.id],
                email: [customer.email],
              })
            );
          }
        });
      }
    });

    this.subscriptions.push(sub);
  }

  removeAgent(index: number): void {
    this.agents.removeAt(index);
  }

  removeCustomer(index: number): void {
    this.customers.removeAt(index);
  }

  save(): void {
    if (this.organizationForm.invalid) {
      this.organizationForm.markAllAsTouched();
      if (this.organizationForm.get('name')?.invalid) {
        this.toastr.error(this.translateService.instant('organizations.name_required'));
      }
      if (this.organizationForm.get('agents')?.hasError('minLength')) {
        this.toastr.error(this.translateService.instant('organizations.agents_required'));
      }
      if (this.organizationForm.get('customers')?.hasError('minLength')) {
        this.toastr.error(this.translateService.instant('organizations.customers_required'));
      }

      return;
    }

    const payload = {
      name: this.organizationForm.value.name,
      agents: this.agents.value.map((agent: any) => ({
        // include {id, role} in the request
        id: agent.id,
        role: agent.role,
      })),
      customers: this.customers.value.map((customer: any) => ({
        // include {id} in the request
        id: customer.id,
      })),
    };

    this.apiService
      .triggerApiRequest<
        BaseResponse<{ id: number; name: string }>
      >(EndPoint.ADD_ORGANIZATION, HttpVerb.POST, null, payload)
      .subscribe({
        next: response => {
          if (response.status.code == ResponseStatusEnum.Success) {
            this.toastr.success(response.status.message);
            this.router.navigate(['/organizations']);
          } else {
            this.toastr.error(response.status.message);
          }
        },
      });
  }

  cancel(): void {
    this.router.navigate(['/organizations']);
  }

  getRoleLabel(role: any): string {
    return this.agentRoleOptions.find(x => x.value === role)?.labelKey ?? "";
  }
}
