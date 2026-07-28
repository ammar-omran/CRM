import { Component, DestroyRef, EventEmitter, Output, inject, Input, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatDivider } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { PageHeaderComponent } from '@shared';
import { EndPoint, HttpVerb } from '@shared/enums';
import { TranslateModule } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { TranslateService } from '@ngx-translate/core';
import { ApiService } from '@shared/services/api.service';
import { HelperService } from '@shared/services/helper.service';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { AttachmentConfigs } from '@shared/interfaces/attachment-configs';
import { AttachmentUploaderComponent } from "@shared/components/attachment-uploader/attachment-uploader.component";
import { environment as env } from '@env/environment';
import { AttachmentType } from '@shared/Enums/attachment-type';

@Component({
  selector: 'app-add-comment',
  standalone: true,
  templateUrl: './add-comment.component.html',
  styleUrls: ['./add-comment.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDivider,
    PageHeaderComponent,
    TranslateModule,
    AttachmentUploaderComponent
  ]
})
export class AddCommentComponent implements OnInit {
  @Input() showHeader: boolean = true;
  @Input() showEmptyState: boolean = true;
  @Output() commentAdded = new EventEmitter<void>();
  @ViewChild('attachmentUploader') attachmentUploaderRef?: AttachmentUploaderComponent;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private api = inject(ApiService);
  private toastr = inject(ToastrService);
  private translate = inject(TranslateService);
  private destroyRef = inject(DestroyRef);

  @Input() ticketIdOverride: number | null = null;
  ticketId!: number;
  commentId: number | null = null;
  uploadedAttachments: { id: any; name: string }[] = [];
  // Attachments Setup
  attachmentType = AttachmentType.comment;
  onAttachmentsChanged(attachments: { id: number; name: string }[]) {
    this.uploadedAttachments = attachments;
  }

  comment = new FormControl<string | null>('', {
    nonNullable: false,
    validators: [Validators.required, Validators.maxLength(1000)]
  });

  submitting = false;

  submit() {
    const text = (this.comment.value || '').trim();
    if (!text) {
      // Only show inline validation, no toast
      const currentErrors = this.comment.errors || {};
      this.comment.setErrors({ ...currentErrors, required: true });
      this.comment.markAsTouched();
      return;
    }

    const endpoint = HelperService.formatEndpoint(
      EndPoint.ADD_TICKET_COMMENT,
      { ticketId: this.ticketId }
    ) as EndPoint;

    const tryPost = (ep: EndPoint) => {
      // Send comment with flag to prevent backend from sending email notifications
      this.api
        .triggerApiRequest(ep, HttpVerb.POST, undefined, {
          content: text,
          sendEmailNotification: false // Tell backend NOT to send email for comments
        })
        .pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe({
          next: (response: any) => {
            this.submitting = false;
            const lang = this.translate.currentLang || localStorage.getItem('lang') || 'en-US';
            const isArabic = (lang || '').toLowerCase().startsWith('ar');
            const candidates = isArabic
              ? [
                response ?.status ?.messageAr,
                response ?.messageAr,
                response ?.msgAr,
                response ?.status ?.message,
                response ?.message,
                response ?.msg,
              ]
              : [
                response ?.status ?.message,
                response ?.message,
                response ?.msg,
                response ?.status ?.messageAr,
                response ?.messageAr,
                response ?.msgAr,
              ];
            const apiMessage = candidates.find(v => typeof v === 'string' && v.trim().length > 0);
            const successMessage = apiMessage || this.translate.instant('reply_sent');
            this.commentId = response.id;
            // Hide the empty state after comment is added
            this.showEmptyState = false;

            // assign attachments to Comment
            this.uploadedAttachments.forEach(att => {
              const setAttachmentReferenceEndpoint = HelperService.formatEndpoint(
                EndPoint.SET_ATTACHMENT_REFERENCE,
                { attachmentId: att.id, referenceId: this.commentId ?? 0 }
              ) as EndPoint;
              this.api
                .triggerApiRequest(setAttachmentReferenceEndpoint, HttpVerb.PATCH)
                .subscribe();
            });

            this.toastr.success(successMessage);
            // Notify parent to refresh comments immediately
            this.commentAdded.emit();
            // Reset input and clear attachments for next comment
            this.comment.reset('');
            this.uploadedAttachments = [];
            this.attachmentUploaderRef?.clearAttachments();
          },
          error: () => {
            this.submitting = false;
            // Global error interceptor will show the localized API message
          },
        });
    };

    this.submitting = true;
    tryPost(endpoint);
  }

  ngOnInit(): void {
    const routeId = Number(this.route.snapshot.paramMap.get('ticketId'));
    this.ticketId = this.ticketIdOverride != null ? this.ticketIdOverride : routeId;
  }

  cancel() {
    this.router.navigate(['/tickets', this.ticketId, 'details']);
  }
}


