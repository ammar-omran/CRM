import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatIconModule } from '@angular/material/icon';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { EmailNotificationService, TicketEmailNotification } from '@shared';
import { HelperService } from '@shared/services/helper.service';
import { MtxButtonModule } from '@ng-matero/extensions/button';
import { AttachmentUploaderComponent } from '@shared/components/attachment-uploader/attachment-uploader.component';
import { AttachmentType } from '@shared/Enums/attachment-type';
import { TranslateModule } from '@ngx-translate/core';

// ─── Interfaces ──────────────────────────────────────────────────────────────

/** A title that belongs to a specific category */
interface TicketTitle {
  id: number;
  title: string;
  defaultSeverityId: number | null;
  isOther: boolean;
}

interface TicketType {
  id: number;
  name: string;
}

/** Category as returned by GET /api/Ticket/lookup/Catagories */
interface TicketCategory {
  id: number;
  name: string;
}

@Component({
  selector: 'app-create-ticket',
  standalone: true,
  templateUrl: './create-ticket.component.html',
  styleUrls: ['./create-ticket.component.scss'],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    MtxButtonModule,
    AttachmentUploaderComponent,
    TranslateModule,
  ],
})
export class CreateTicketComponent implements OnInit {
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);
  private emailNotificationService = inject(EmailNotificationService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private snackBar = inject(MatSnackBar);

  ticketForm!: FormGroup;
  customerId: number = 1;
  isSubmitting = false;
  uploadedAttachments: { id: any; name: string }[] = [];
  ticketId: number | null = null;
  attachmentType = AttachmentType.ticket;

  // ─── Category state ──────────────────────────────────────────────────────────
  ticketCategories: TicketCategory[] = [];
  isCategoriesLoading = false;

  // ─── Title state ─────────────────────────────────────────────────────────────
  /** All titles for the currently selected category (including "Other" sentinel) */
  filteredTitles: TicketTitle[] = [];
  isTitlesLoading = false;

  // ─── Static lookup data ──────────────────────────────────────────────────────
  ticketTypes: TicketType[] = [
    { id: 1, name: 'Complaint' },
    { id: 2, name: 'Inquiry' },
  ];

  // ─── Computed helpers ────────────────────────────────────────────────────────

  /** True when the customer chose the "Other" title option. */
  get isOtherSelected(): boolean {
    const titleId = this.ticketForm?.get('titleId')?.value;
    if (!titleId || !this.filteredTitles.length) return false;
    const found = this.filteredTitles.find(t => t.id === Number(titleId));
    return found?.isOther === true;
  }

  /** Severity id linked to the currently selected title; null when "Other". */
  private get selectedSeverityId(): number | null {
    const titleId = this.ticketForm?.get('titleId')?.value;
    if (!titleId) return null;
    const found = this.filteredTitles.find(t => t.id === Number(titleId));
    return found?.defaultSeverityId ?? null;
  }

  // ─── Lifecycle ───────────────────────────────────────────────────────────────

  ngOnInit(): void {
    this.route.paramMap.pipe(takeUntilDestroyed(this.destroyRef)).subscribe(params => {
      const customerId = params.get('customerId');
      this.customerId = customerId ? Number(customerId) : 1;
      this.initializeForm();
    });

    this.loadCategories();
  }

  // ─── Form setup ──────────────────────────────────────────────────────────────

  private initializeForm(): void {
    if (!this.ticketForm) {
      this.ticketForm = this.fb.group({
        typeId: ['', Validators.required],
        categoryId: ['', Validators.required],
        titleId: ['', Validators.required],
        customTitle: [''], // validators added/removed dynamically
        description: ['', [Validators.required, Validators.maxLength(1000)]],
      });

      // React to category selection changes → load titles for that category
      this.ticketForm
        .get('categoryId')
        ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(categoryId => this.onCategoryChange(categoryId));

      // React to title selection changes
      this.ticketForm
        .get('titleId')
        ?.valueChanges.pipe(takeUntilDestroyed(this.destroyRef))
        .subscribe(() => this.onTitleChange());
    } else {
      this.ticketForm.reset();
      this.filteredTitles = [];
    }
  }

  // ─── Data loading ────────────────────────────────────────────────────────────

  /** Fetch categories from GET /api/Ticket/lookup/Catagories */
  private loadCategories(): void {
    this.isCategoriesLoading = true;
    this.apiService
      .triggerApiRequest<any>(EndPoint.GET_TICKET_CATEGORIES, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isCategoriesLoading = false;
          let rawList: any[] = [];
          if (Array.isArray(res)) {
            rawList = res;
          } else if (res?.data && Array.isArray(res.data)) {
            rawList = res.data;
          } else if (res?.items && Array.isArray(res.items)) {
            rawList = res.items;
          }
          this.ticketCategories = rawList.map((c: any) => ({
            id: c.id,
            name: c.name,
          }));
        },
        error: () => {
          this.isCategoriesLoading = false;
          this.snackBar.open('Failed to load categories. Please refresh.', 'Close', {
            duration: 5000,
            panelClass: ['error-snackbar'],
          });
        },
      });
  }

  /**
   * Called when category selection changes.
   * Clears existing title selection and loads titles for the new category
   * via GET /api/Ticket/categories/{categoryId}/titles.
   */
  private onCategoryChange(categoryId: any): void {
    // Reset title field and filtered list whenever category changes
    this.ticketForm.get('titleId')?.setValue('');
    this.filteredTitles = [];

    if (!categoryId) return;

    // Build the endpoint with the selected categoryId
    const endpoint = EndPoint.GET_TICKET_TITLES_BY_CATEGORY.replace(
      '{categoryId}',
      String(categoryId)
    ) as EndPoint;

    this.isTitlesLoading = true;
    this.apiService
      .triggerApiRequest<any>(endpoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.isTitlesLoading = false;
          let rawList: any[] = [];
          if (Array.isArray(res)) {
            rawList = res;
          } else if (res?.data && Array.isArray(res.data)) {
            rawList = res.data;
          } else if (res?.items && Array.isArray(res.items)) {
            rawList = res.items;
          }

          const titles: TicketTitle[] = rawList.map((t: any) => ({
            id: t.id,
            title: t.title,
            defaultSeverityId: t.defaultSeverityId ?? null,
            isOther: t.isOther === true,
          }));

          // Always ensure "Other" appears exactly once at the end.
          // id=-1 avoids JS 0-is-falsy; converted to 0 on submit.
          if (!titles.some(t => t.isOther)) {
            titles.push({
              id: -1,
              title: 'Other',
              defaultSeverityId: null,
              isOther: true,
            });
          }

          this.filteredTitles = titles;
        },
        error: () => {
          this.isTitlesLoading = false;
          this.snackBar.open(
            'Failed to load titles for this category. Please try again.',
            'Close',
            {
              duration: 5000,
              panelClass: ['error-snackbar'],
            }
          );
        },
      });
  }

  private onTitleChange(): void {
    const customTitleControl = this.ticketForm.get('customTitle');
    if (!customTitleControl) return;

    if (this.isOtherSelected) {
      customTitleControl.setValidators([Validators.required, Validators.maxLength(200)]);
    } else {
      customTitleControl.clearValidators();
      customTitleControl.setValue('');
    }
    customTitleControl.updateValueAndValidity();
  }

  // ─── Attachment handler ──────────────────────────────────────────────────────

  onAttachmentsChanged(attachments: { id: number; name: string }[]): void {
    this.uploadedAttachments = attachments;
  }

  // ─── Submit ──────────────────────────────────────────────────────────────────

  onSubmit(): void {
    if (!this.ticketForm.valid) {
      this.markFormGroupTouched();
      return;
    }

    const formValue = this.ticketForm.value;

    // Basic guard
    if (
      !formValue.typeId ||
      !formValue.categoryId ||
      !formValue.titleId ||
      !formValue.description
    ) {
      this.snackBar.open('Please fill in all required fields', 'Close', {
        duration: 5000,
        panelClass: ['error-snackbar'],
      });
      return;
    }

    this.isSubmitting = true;

    const customerEmail = localStorage.getItem('customerEmail');
    const customerName = localStorage.getItem('customerName');

    // Resolve title text — free-text when "Other", else the predefined name
    const selectedTitle = this.filteredTitles.find(t => t.id === Number(formValue.titleId));
    const titleText = this.isOtherSelected
      ? formValue.customTitle || ''
      : selectedTitle?.title || '';

    // Severity: null for "Other", backend-mapped value for predefined titles
    const severityId = this.isOtherSelected ? null : this.selectedSeverityId;

    const requestBody = {
      categoryId: Number(formValue.categoryId),
      typeId: Number(formValue.typeId),
      titleId: this.isOtherSelected ? 0 : Number(formValue.titleId), // 0 = "Other" signal for backend
      // 'title' satisfies the backend's required [Required] string property
      title: titleText,
      customTitle: this.isOtherSelected ? formValue.customTitle || '' : '',
      description: formValue.description,
      customerEmail: customerEmail || 'customer@example.com',
      // Send severtyId (backend typo preserved); null when customer chose "Other"
      severtyId: severityId,
      customerId: Number(this.customerId),
      customerName: customerName || 'Customer',
      userId: 0,
      userName: 'System',
      createdBy: 1,
      createdByName: 'Current User',
      updatedBy: 1,
      updatedByName: 'Current User',
      status: 1,
    };

    this.apiService
      .triggerApiRequest<any>(EndPoint.AddTicket, HttpVerb.POST, undefined, requestBody)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: response => {
          this.ticketId = response?.id !== undefined ? response.id : 0;

          // Assign any pre-uploaded attachments to the new ticket
          this.uploadedAttachments.forEach(att => {
            const ep = HelperService.formatEndpoint(EndPoint.SET_ATTACHMENT_REFERENCE, {
              attachmentId: att.id,
              referenceId: this.ticketId ?? 0,
            }) as EndPoint;
            this.apiService.triggerApiRequest(ep, HttpVerb.PATCH).subscribe();
          });

          // Show success message with the ticket ID
          this.snackBar.open(`✅ Ticket #${this.ticketId} created successfully!`, 'Close', {
            duration: 5000,
            panelClass: ['success-snackbar'],
          });

          const ticketRef = `TKT-${this.ticketId}-${new Date().getFullYear()}`;
          const creationDate = new Date().toLocaleDateString();

          // Navigate to the ticket list
          this.router.navigate(['/tickets', this.customerId], {
            queryParams: { refresh: 'true' },
            queryParamsHandling: 'merge',
          });

          // Send email notification after successful creation
          this.sendTicketCreationEmail(
            ticketRef,
            creationDate,
            formValue,
            titleText,
            selectedTitle?.title
          );
        },
        error: (error: any) => {
          this.isSubmitting = false;
          let errorMessage = 'Failed to create ticket. Please try again.';

          if (error?.error?.errors) {
            errorMessage = Object.entries(error.error.errors)
              .map(([field, messages]) => {
                const fieldName = this.getFieldDisplayName(field) || field;
                return `${fieldName}: ${Array.isArray(messages) ? (messages as string[]).join(', ') : String(messages)}`;
              })
              .join('\n');
          } else if (error?.error?.message) {
            errorMessage = error.error.message;
          } else if (typeof error?.error === 'string') {
            errorMessage = error.error;
          } else if (error?.status === 0) {
            errorMessage = 'Unable to connect to the server. Please check your connection.';
          } else if (error?.status >= 500) {
            errorMessage = 'A server error occurred. Please try again later.';
          }

          this.snackBar.open(errorMessage, 'Close', {
            duration: 15000,
            panelClass: ['error-snackbar'],
          });
        },
      });
  }

  // ─── Email notification ──────────────────────────────────────────────────────

  private sendTicketCreationEmail(
    ticketRef: string,
    creationDate: string,
    formValue: any,
    titleText: string,
    predefinedTitleName?: string
  ): void {
    const customerEmail = localStorage.getItem('customerEmail');
    const customerName = localStorage.getItem('customerName');

    if (!customerEmail) return;

    const typeLabel = this.ticketTypes.find(t => t.id == formValue.typeId)?.name || 'Unknown';
    const categoryLabel =
      this.ticketCategories.find(c => c.id == formValue.categoryId)?.name || 'Unknown';

    const notification: TicketEmailNotification = {
      customerEmail,
      customerName: customerName || 'Customer',
      ticketId: parseInt(ticketRef.split('-')[1]) || 0,
      ticketReference: ticketRef,
      subject: titleText,
      description: formValue.description,
      category: categoryLabel,
      service: '',
      type: typeLabel,
      severity: '',
      creationDate,
    };

    this.emailNotificationService
      .sendTicketCreationEmailViaApiService(notification)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: () => {
          this.snackBar.open('📧 Confirmation email sent', 'Close', {
            duration: 3000,
            panelClass: ['success-snackbar'],
          });
        },
        error: () => {
          // Email failure is non-blocking; ticket was already created.
        },
      });
  }

  // ─── Helpers ─────────────────────────────────────────────────────────────────

  getFieldError(fieldName: string): string {
    const control = this.ticketForm.get(fieldName);
    if (control?.errors && control.touched) {
      if (control.errors['required']) {
        return `This field is required: ${this.getFieldDisplayName(fieldName)} | هذا الحقل مطلوب: ${this.getFieldDisplayNameArabic(fieldName)}`;
      }
      if (control.errors['maxlength']) {
        return fieldName === 'description'
          ? 'Description is too long. Maximum 1000 characters allowed.'
          : 'Text is too long. Maximum 200 characters allowed.';
      }
    }
    return '';
  }

  private markFormGroupTouched(): void {
    Object.keys(this.ticketForm.controls).forEach(key => {
      this.ticketForm.get(key)?.markAsTouched();
    });
  }

  private getFieldDisplayName(fieldName: string): string {
    const names: Record<string, string> = {
      typeId: 'Ticket Type',
      categoryId: 'Category',
      titleId: 'Title',
      customTitle: 'Custom Title',
      description: 'Description',
    };
    return names[fieldName] || fieldName;
  }

  private getFieldDisplayNameArabic(fieldName: string): string {
    const names: Record<string, string> = {
      typeId: 'نوع التذكرة',
      categoryId: 'الفئة',
      titleId: 'العنوان',
      customTitle: 'عنوان مخصص',
      description: 'الوصف',
    };
    return names[fieldName] || fieldName;
  }

  onCancel(): void {
    this.router.navigate(['/tickets', this.customerId]);
  }

  getCategoryIcon(categoryName: string): string {
    const iconMap: Record<string, string> = {
      'Metering': 'speed',
      'Billing and Payments': 'payment',
      'Installation': 'construction',
      'Customer Service': 'support_agent',
    };
    return iconMap[categoryName] || 'folder';
  }
}
