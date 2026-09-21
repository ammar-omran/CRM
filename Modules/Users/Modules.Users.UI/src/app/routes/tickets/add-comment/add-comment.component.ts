import { Component, DestroyRef, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EndPoint, HttpVerb } from '@shared/enums';
import { HelperService } from '@shared/services/helper.service';
import { ApiService } from '@shared/services/api.service';
import { AttachmentUploaderComponent } from '@shared/components/attachment-uploader/attachment-uploader.component';
import { AttachmentType } from '@shared/Enums/attachment-type';
import { RbacService } from '@core/authentication/rbac.service';

@Component({
  selector: 'app-add-comment',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    TranslatePipe,
    AttachmentUploaderComponent,
  ],
  templateUrl: './add-comment.component.html',
  styleUrls: ['./add-comment.component.scss'],
})
export class AddCommentComponent implements OnInit {
  @Input() ticketId!: number;
  @Input() showNoComments = false;
  @Output() commentAdded = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly translate = inject(TranslateService);
  private readonly toastr = inject(ToastrService);
  private readonly rbac = inject(RbacService);
  private readonly destroyRef = inject(DestroyRef);

  commentForm = this.fb.nonNullable.group({
    content: ['', [Validators.required, Validators.maxLength(1000)]],
  });

  isSubmitting = false;
  uploadedAttachments: { id: number; name: string }[] = [];
  attachmentType = AttachmentType.comment;
  commentId: number | null = null;

  get content() {
    return this.commentForm.get('content');
  }

  onAttachmentsChanged(attachments: { id: number; name: string }[]): void {
    this.uploadedAttachments = attachments;
  }

  ngOnInit(): void {
    this.route.params.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      if (!this.ticketId && params?.ticketId) {
        this.ticketId = Number(params.ticketId);
      }
    });
  }

  onSubmit(): void {
    if (this.commentForm.invalid) {
      this.commentForm.markAllAsTouched();
      return;
    }
    this.isSubmitting = true;
    const content = this.commentForm.value.content?.trim() ?? '';
    const isAdmin = this.rbac.isAdmin() || this.rbac.isSupervisor();

    const endpoint = HelperService.formatEndpoint(EndPoint.ADD_TICKET_COMMENT, {
      ticketId: this.ticketId,
    }) as EndPoint;

    // Backend AddCommentRequest { Content, IsAdmin } — CreatedBy/Name derived from JWT via CurrentUser
    this.api
      .triggerApiRequest<{ id: number; Message?: string; MessageAr?: string }>(endpoint, HttpVerb.POST, undefined, {
        Content: content,
        IsAdmin: isAdmin,
      })
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isSubmitting = false;
          const newId = (res as any)?.id ?? (res as any)?.Id ?? null;
          if (newId) {
            this.commentId = Number(newId);
            this.uploadedAttachments.forEach(att => {
              const ep = HelperService.formatEndpoint(EndPoint.SET_ATTACHMENT_REFERENCE, {
                attachmentId: att.id,
                referenceId: this.commentId!,
              }) as EndPoint;
              this.api.triggerApiRequest(ep, HttpVerb.PATCH).subscribe({
                error: () => console.warn('Failed to link attachment', att.id),
              });
            });
          }
          const msg = (this.translate.currentLang()?.startsWith('ar') ?? false)
            ? (res as any)?.MessageAr ?? 'تم إرسال الرد'
            : (res as any)?.Message ?? 'Comment sent';
          this.toastr.success(msg);
          this.commentForm.reset({ content: '' });
          this.uploadedAttachments = [];
          this.commentAdded.emit();
          if (this.router.url.includes('/add-comment')) {
            this.router.navigate(['/tickets', this.ticketId, 'details']);
          }
        },
        error: err => {
          this.isSubmitting = false;
          const msg = err?.error?.Status?.Message || err?.error?.message || 'Failed to add comment';
          this.toastr.error(msg);
        },
      });
  }

  onCancel(): void {
    if (this.cancelled.observed) {
      this.cancelled.emit();
      return;
    }
    this.router.navigate(['/tickets', this.ticketId, 'details']);
  }
}
