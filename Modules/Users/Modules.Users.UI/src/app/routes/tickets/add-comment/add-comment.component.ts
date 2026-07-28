import { Component, EventEmitter, Input, OnInit, Output, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient } from '@angular/common/http';
import { environment } from '@env/environment';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { NgxRolesService } from 'ngx-permissions';
import { AuthService } from '@core/authentication/auth.service';
// Removed email notification service usage because API endpoint doesn't exist
import { TicketDetails } from '@shared/interfaces/ticket-details';
import { AttachmentType } from '@shared/Enums/attachment-type';
import { EndPoint, HttpVerb } from '@shared/enums';
import { HelperService } from '@shared/services/helper.service';
import { ApiService } from '@shared/services/api.service';
import { AttachmentUploaderComponent } from '@shared/components/attachment-uploader/attachment-uploader.component';

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
    TranslateModule,
    AttachmentUploaderComponent
  ],
  templateUrl: './add-comment.component.html',
  styleUrls: ['./add-comment.component.scss'],
})
export class AddCommentComponent implements OnInit {
  @Input() ticketId!: number;
  @Input() showNoComments: boolean = false;
  @Output() commentAdded = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();
  // If used standalone via route, will read from route params in ngOnInit
  commentForm: FormGroup;
  isSubmitting = false;
  currentLanguage = 'en-US';
  isAdmin = false;
  adminId: number | null = null;
  adminName: string | null = null;
  ticketDetails: TicketDetails | null = null;
  commentId: number | null = null;
  uploadedAttachments: { id: any; name: string }[] = [];
  // Attachments Setup
  attachmentType = AttachmentType.comment;
  onAttachmentsChanged(attachments: { id: number; name: string }[]) {
    this.uploadedAttachments = attachments;
  }

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private api = inject(ApiService);
  private translateService = inject(TranslateService);
  private toastr = inject(ToastrService);
  private rolesService = inject(NgxRolesService);
  private authService = inject(AuthService);
  // Email notification service removed

  constructor() {
    this.commentForm = this.fb.group({
      content: ['', [Validators.required, Validators.minLength(1)]],
    });
  }

  ngOnInit(): void {
    // If ticketId is not passed as Input (standalone route usage), read it from params
    this.route.params.subscribe(params => {
      if (!this.ticketId && params && params.ticketId) {
        this.ticketId = +params.ticketId;
      }
      if (this.ticketId) {
        this.loadTicketDetails();
      }
    });

    // Get current language and subscribe to language changes
    this.updateCurrentLanguage();

    // Determine if current user has ADMIN role
    try {
      const roles = this.rolesService.getRoles ?.();
      this.isAdmin = !!roles && Object.prototype.hasOwnProperty.call(roles, 'ADMIN');
    } catch {
      this.isAdmin = false;
    }

    // Capture admin identity from auth state (fallbacks to backend defaults if missing)
    this.authService.user().subscribe(user => {
      const parsedId = Number(user ?.id);
      this.adminId = Number.isFinite(parsedId) ? parsedId : null;
      this.adminName = (user ?.name || user ?.userName) ?? null;
    });


    // Subscribe to language changes
    this.translateService.onLangChange.subscribe(event => {
      this.currentLanguage = event.lang;
      this.updateCurrentLanguage();
    });

    // Also check language on every form submission to ensure it's current
    this.commentForm.valueChanges.subscribe(() => {
      this.updateCurrentLanguage();
    });
  }

  private loadTicketDetails(): void {
    const url = `${environment.ApiUrl}/Ticket/GetTicketDetails/${this.ticketId}`;
    this.http.get<TicketDetails>(url).subscribe({
      next: ticket => {
        this.ticketDetails = ticket;
      },
      error: error => {
        console.error('Error loading ticket details:', error);
      },
    });
  }

  private updateCurrentLanguage(): void {
    // Get the current language directly from the translate service
    const currentLang = this.translateService.currentLang;


    // Set the language based on what the translate service reports
    this.currentLanguage = currentLang || 'en';

    this.currentLanguage = currentLang || 'en';

  }

  get content() {
    return this.commentForm.get('content');
  }

  // Removed sendEmailNotification because corresponding backend endpoint is not available

  onSubmit(): void {
    if (this.commentForm.invalid) {
      return;
    }

    this.isSubmitting = true;
    const url = `${environment.ApiUrl}/Ticket/${this.ticketId}/addComment`;
    const body: any = {
      content: this.commentForm.value.content,
    };

    // Minimal admin payload per backend contract
    if (this.isAdmin) {
      body.isAdmin = true;
      // Explicitly set superadmin identity and assignment as requested
      body.createdBy = 1;
      body.createdByName = 'superadmin';
      // Also send PascalCase to align with backend DB column naming
      body.CreatedBy = 1;
      body.CreatedByName = 'superadmin';
      // Update the ticket's AssignedTo field to superadmin - try multiple variations
      body.assignedTo = 'superadmin';
      body.AssignedTo = 'superadmin';
      body.assignedToName = 'superadmin';
      body.AssignedToName = 'superadmin';
      body.assignedToId = 1;
      body.AssignedToId = 1;
      // Additional fields that might be needed for assignment update
      body.ticketAssignedTo = 'superadmin';
      body.TicketAssignedTo = 'superadmin';
      body.assignTo = 'superadmin';
      body.AssignTo = 'superadmin';
      body.assignToName = 'superadmin';
      body.AssignToName = 'superadmin';
    }


    this.http.post(url, body).subscribe({
      next: (response: any) => {
        this.isSubmitting = false;


        // Get the most current language directly from the service
        const currentLang = this.translateService.currentLang;





        // Show success message based on current language
        let message = 'Comment added successfully';

        // Check multiple language indicators - handle both 'ar' and 'ar-EG' formats
        if ((currentLang ?.startsWith('ar') || this.currentLanguage ?.startsWith('ar')) && response.messageAr) {
          message = response.messageAr;
        } else if (response.message) {
          message = response.message;
        }

        this.toastr.success(message);

        // Email notification disabled: backend endpoint not available

        // Clear the form and attachments after successful submission
        this.commentForm.reset({ content: '' });
        this.commentForm.markAsPristine();
        this.commentForm.markAsUntouched();
        this.uploadedAttachments = [];

        // Emit to parent to refresh comments when embedded
        this.commentAdded.emit();

        // If used via route, navigate back to details
        if (this.router.url.includes('/add-comment')) {
          const commentsUrl = `${environment.ApiUrl}/Ticket/GetTicketComments/${this.ticketId}`;
          this.http.get(commentsUrl).subscribe({
            next: () => this.router.navigate(['/tickets', this.ticketId, 'details']),
            error: () => this.router.navigate(['/tickets', this.ticketId, 'details'])
          });
        }
      },
      error: error => {
        this.isSubmitting = false;
        console.error('Error adding comment:', error);


        // Show error message based on current language
        let errorMessage = 'Failed to add comment. Please try again.';
        if (this.currentLanguage === 'ar') {
          errorMessage = 'فشل في إضافة التعليق. يرجى المحاولة مرة أخرى.';
        }


        this.toastr.error(errorMessage);
      },
    });
  }

  onCancel(): void {
    // Notify parent when embedded; otherwise fallback to navigate
    if (this.commentAdded.observed || this.cancelled.observed) {
      this.cancelled.emit();
      return;
    }
    this.router.navigate(['/tickets', this.ticketId, 'details']);
  }
}
