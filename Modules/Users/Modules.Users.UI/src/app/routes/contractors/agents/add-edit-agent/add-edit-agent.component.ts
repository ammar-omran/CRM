import { Component, DestroyRef, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { TranslatePipe } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';

@Component({
  selector: 'app-add-edit-agent',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslatePipe,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
  ],
  templateUrl: './add-edit-agent.component.html',
  styleUrl: './add-edit-agent.component.scss',
})
export class AddEditAgentComponent {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly toastr = inject(ToastrService);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);

  isSubmitting = false;

  agentForm = this.fb.nonNullable.group({
    userId: ['', [Validators.required]],
  });

  onSubmit(): void {
    if (this.agentForm.invalid) {
      this.agentForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    const { userId } = this.agentForm.getRawValue();

    // Backend POST api/agents expects { UserId: string } — links existing Identity user as agent
    this.api
      .triggerApiRequest(EndPoint.ADD_AGENT, HttpVerb.POST, undefined, { UserId: userId })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.toastr.success('Agent created successfully');
          this.router.navigate(['/contractors']);
        },
        error: err => {
          const msg = err?.error?.Status?.Message || err?.error?.message || 'Failed to create agent';
          this.toastr.error(msg);
          this.isSubmitting = false;
        },
        complete: () => (this.isSubmitting = false),
      });
  }
}
