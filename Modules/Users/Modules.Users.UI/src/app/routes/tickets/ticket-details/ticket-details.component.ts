import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PageHeaderComponent } from '@shared';
import { MatTableModule } from '@angular/material/table';
import { MatTabGroup, MatTab } from '@angular/material/tabs';
import { TicketHistoryComponent } from '../ticket-history/ticket-history.component';
import { MatCard, MatCardActions, MatCardContent, MatCardTitle } from '@angular/material/card';
import { MatDivider } from '@angular/material/divider';
import { EndPoint, HttpVerb } from '@shared/enums';
import { TicketDetails } from '@shared/interfaces/ticket-details';
import { HelperService } from '@shared/services/helper.service';
import { ApiService } from '@shared/services/api.service';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { MatButtonModule } from '@angular/material/button';
import { CommentComponent } from '@shared/utils/comment/comment.component';
import { AddCommentComponent } from '../add-comment/add-comment.component';
import { HasPermissionDirective } from '@shared';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, forkJoin, map, of, switchMap, throwError } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  standalone: true,
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss'],
  imports: [
    CommonModule,
    PageHeaderComponent,
    MatTableModule,
    MatTabGroup,
    MatTab,
    TicketHistoryComponent,
    MatCard,
    MatCardActions,
    MatCardContent,
    MatCardTitle,
    MatDivider,
    TranslatePipe,
    MatButtonModule,
    CommentComponent,
    AddCommentComponent,
    HasPermissionDirective,
  ],
})
export class TicketDetailsComponent implements OnInit {
  ticketId!: number;
  showHistoryTab = false;
  hasTicketAttachments = false;
  attachmentUrl: string | null = null;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  private translate = inject(TranslateService);
  private toastr = inject(ToastrService);
  ticket: TicketDetails | null = null;
  error: string | null = null;
  loading = true;

  ngOnInit(): void {
    const id = parseInt(this.route.snapshot.params.ticketId, 10);
    if (!id) {
      return;
    }
    this.ticketId = id;
    this.loadTicketDetails();
  }

  private loadTicketDetails(): void {
    const endpoint = HelperService.formatEndpoint(EndPoint.GET_TICKET_DETAILS, {
      ticketId: this.ticketId,
    }) as EndPoint;
    this.getComments();
    this.apiService.triggerApiRequest<TicketDetails>(endpoint, HttpVerb.GET).subscribe({
      next: data => {
        this.ticket = data;
        this.loading = false;
        this.checkTicketAttachments(this.ticketId);
      },
      error: err => {
        this.error = 'Failed to load ticket details.';
        this.loading = false;
        console.error('API error:', err);
      },
    });
  }

  refreshTicketDetails(): void {
    this.loadTicketDetails();
  }

  onTabChange(event: any): void {
    if (event.index === 1) {
      this.showHistoryTab = true;
    }
  }

  navigateToAddComment(): void {
    this.router.navigate(['/tickets', this.ticketId, 'add-comment']);
  }

  ticketComments: any[] = [];
  adminId: string = localStorage.getItem('adminId') || 'no';

  getComments() {
    const endpoint = HelperService.formatEndpoint(EndPoint.GET_TICKET_COMMENTS, {
      ticketId: this.ticketId,
    }) as EndPoint;

    this.apiService.triggerApiRequest<any>(endpoint, HttpVerb.GET).subscribe({
      next: (response: any) => {
        // New backend returns PaginationResponse or direct array: handle both
        // Legacy shape: { status, ticketComments } vs new: [{ CommentId, CreatedById, ... }] or { items: [...] }
        const raw = response?.data ?? response?.Data ?? response?.items ?? response?.Items ?? response;
        const list = Array.isArray(raw) ? raw : raw?.ticketComments ?? raw?.TicketComments ?? [];
        const arr = Array.isArray(list) ? list : [];
        if (arr.length === 0 && Array.isArray(response) && response.length === 0) {
          this.ticketComments = [];
          return;
        }
        // If raw was already the array (new backend), use it; otherwise use list
        const source = arr.length ? arr : Array.isArray(raw) ? raw : [];
        this.ticketComments = source.map((comment: any) => {
          const createdById = comment.createdById ?? comment.CreatedById ?? comment.commenter ?? comment.Commenter ?? 0;
          const createdByName =
            comment.createdByName ?? comment.CreatedByName ?? comment.author ?? 'Unknown';
          const haveAtt = comment.haveAttachments ?? comment.HaveAttachments ?? comment.isAdmin ?? false;
          // Backend now returns IsAdmin + CreatedByName directly
          const rawIsAdmin = comment.isAdmin ?? comment.IsAdmin ?? null;
          let role: string;
          if (rawIsAdmin !== null && rawIsAdmin !== undefined) {
            role = rawIsAdmin ? 'Support' : 'Customer';
          } else if (String(createdByName).toLowerCase() === 'superadmin') {
            role = 'Support';
          } else {
            role = 'Customer';
          }
          return {
            id: comment.commentId ?? comment.CommentId ?? comment.id ?? comment.Id,
            author: createdByName,
            description: comment.description ?? comment.Description ?? comment.content ?? comment.Content ?? '',
            createdDate: String(comment.createdDate ?? comment.CreatedDate ?? ''),
            HaveAttachments: !!haveAtt,
            createdById,
            role,
          };
        });
        this.enrichCommentsWithAttachmentStatus();
      },
      error: () => {
        this.ticketComments = [];
      },
    });
  }

  handleCommentAdded() {
    this.getComments();
  }

  private enrichCommentsWithAttachmentStatus(): void {
    if (!this.ticketComments?.length) return;

    const checks = this.ticketComments.map(comment => {
      const commentId = this.resolveCommentId(comment);
      if (commentId == null) return of(comment.HaveAttachments ?? false);
      if (comment.HaveAttachments === true) return of(true);

      const endpoint = (EndPoint.GET_ATTACHMENT_BY_REFERENCE as string).replace(
        '{referenceId}',
        String(commentId)
      ) as EndPoint;

      return this.apiService.triggerApiRequest<any>(endpoint, HttpVerb.GET).pipe(
        map(res => {
          // Backend returns BaseResponse { data: AttachmentDTO[] } — has attachments if data non-empty
          const data = (res as any)?.data ?? res;
          if (Array.isArray(data)) return data.length > 0;
          if (Array.isArray((res as any)?.Data)) return (res as any).Data.length > 0;
          return data != null;
        }),
        catchError(() => of(false))
      );
    });

    forkJoin(checks)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(results => {
        results.forEach((has, i) => {
          if (!this.ticketComments[i].HaveAttachments) {
            this.ticketComments[i].HaveAttachments = has;
          }
        });
      });
  }

  private resolveCommentId(comment: any): number | null {
    const candidate =
      comment?.id ??
      comment?.commentId ??
      comment?.ticketCommentId ??
      comment?.referenceId ??
      comment?.comment_id;
    const numeric = candidate != null ? Number(candidate) : NaN;
    return Number.isFinite(numeric) ? numeric : null;
  }

  openAttachmentInNewTab(): void {
    if (!this.ticketId && this.ticketId !== 0) {
      this.toastr.error('Invalid ticket reference');
      return;
    }
    if (!this.hasTicketAttachments) {
      this.toastr.error(this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT'));
      return;
    }
    this.getFileIdForReference(this.ticketId)
      .pipe(
        switchMap(fileId => {
          const endpoint = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          return this.apiService.triggerApiRequest(endpoint, HttpVerb.GET, undefined, undefined, {
            responseType: 'blob',
            observe: 'response',
            headers: { Accept: '*/*' },
          });
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res?.body instanceof Blob ? res.body : new Blob([res?.body || '']);
        const url = URL.createObjectURL(fileBlob);
        if (fileBlob.type && String(fileBlob.type).startsWith('image/')) {
          this.attachmentUrl = url;
        }
        window.open(url, '_blank');
      });
  }

  viewCommentAttachment(comment: any): void {
    const commentId = this.resolveCommentId(comment);
    if (commentId == null) {
      this.toastr.error('Invalid comment reference');
      return;
    }
    this.getFileIdForReference(commentId)
      .pipe(
        switchMap(fileId => {
          const endpoint = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          return this.apiService.triggerApiRequest(endpoint, HttpVerb.GET, undefined, undefined, {
            responseType: 'blob',
            observe: 'response',
            headers: { Accept: '*/*' },
          });
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res?.body instanceof Blob ? res.body : new Blob([res?.body || '']);
        const url = URL.createObjectURL(fileBlob);
        window.open(url, '_blank');
      });
  }

  downloadCommentAttachment(comment: any): void {
    const commentId = this.resolveCommentId(comment);
    if (commentId == null) {
      this.toastr.error('Invalid comment reference');
      return;
    }
    this.getFileIdForReference(commentId)
      .pipe(
        switchMap(fileId => {
          const endpoint = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          return this.apiService.triggerApiRequest(endpoint, HttpVerb.GET, undefined, undefined, {
            responseType: 'blob',
            observe: 'response',
            headers: { Accept: '*/*' },
          });
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res?.body instanceof Blob ? res.body : new Blob([res?.body || '']);
        const contentDisposition: string = res?.headers?.get?.('content-disposition') || '';
        const filenameMatch = /filename\*=UTF-8''([^;]+)|filename=([^;]+)/i.exec(
          contentDisposition
        );
        const filename = decodeURIComponent(
          (filenameMatch?.[1] || filenameMatch?.[2] || `comment-${commentId}`).replace(/"/g, '')
        );
        const url = URL.createObjectURL(fileBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      });
  }

  downloadAttachment(): void {
    if (!this.ticketId && this.ticketId !== 0) {
      this.toastr.error('Invalid ticket reference');
      return;
    }
    if (!this.hasTicketAttachments) {
      this.toastr.error(this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT'));
      return;
    }
    this.getFileIdForReference(this.ticketId)
      .pipe(
        switchMap(fileId => {
          const endpoint = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          return this.apiService.triggerApiRequest(endpoint, HttpVerb.GET, undefined, undefined, {
            responseType: 'blob',
            observe: 'response',
            headers: { Accept: '*/*' },
          });
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res?.body instanceof Blob ? res.body : new Blob([res?.body || '']);
        const contentDisposition: string = res?.headers?.get?.('content-disposition') || '';
        const filenameMatch = /filename\*=UTF-8''([^;]+)|filename=([^;]+)/i.exec(
          contentDisposition
        );
        const filename = decodeURIComponent(
          (filenameMatch?.[1] || filenameMatch?.[2] || `ticket-${this.ticketId}`).replace(/"/g, '')
        );
        const url = URL.createObjectURL(fileBlob);
        const link = document.createElement('a');
        link.href = url;
        link.download = filename;
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(() => URL.revokeObjectURL(url), 1000);
      });
  }

  /** Extract FileId from Attachments/reference/{referenceId} response.
   *  Backend returns BaseResponse { data: AttachmentDTO[] } where DTO has FileId (PascalCase, may be camelCased).
   *  Handles array, wrapped data, and single object forms. Returns first FileId or null. */
  private extractFileId(refRes: any): number | null {
    if (!refRes) return null;
    // Unwrap BaseResponse wrapper: { data: ... } or { Data: ... }
    if (typeof refRes === 'object' && !Array.isArray(refRes)) {
      const wrapped = (refRes as any).data ?? (refRes as any).Data;
      if (wrapped !== undefined && wrapped !== refRes) {
        const fromWrapped = this.extractFileId(wrapped);
        if (fromWrapped != null) return fromWrapped;
      }
      // case-insensitive fileId lookup on object
      for (const k of Object.keys(refRes)) {
        if (k.toLowerCase() === 'fileid') {
          const v = Number((refRes as any)[k]);
          if (Number.isFinite(v) && v > 0) return v;
        }
      }
      for (const k of Object.keys(refRes)) {
        if (k.toLowerCase() === 'id') {
          const v = Number((refRes as any)[k]);
          if (Number.isFinite(v) && v > 0) return v;
        }
      }
    }
    if (Array.isArray(refRes) && refRes.length > 0) {
      const first = refRes[0];
      for (const k of Object.keys(first)) {
        if (k.toLowerCase() === 'fileid') {
          const v = Number((first as any)[k]);
          if (Number.isFinite(v) && v > 0) return v;
        }
      }
      for (const k of Object.keys(first)) {
        if (k.toLowerCase() === 'id') {
          const v = Number((first as any)[k]);
          if (Number.isFinite(v) && v > 0) return v;
        }
      }
    }
    return null;
  }

  /** Resolve FileId for a referenceId via GET Attachments/reference/{referenceId}.
   *  This is the ONLY supported lookup — per AttachmentsController.cs there is no download by ticketId/commentId. */
  private getFileIdForReference(referenceId: number) {
    const endpoint = (EndPoint.GET_ATTACHMENT_BY_REFERENCE as string).replace(
      '{referenceId}',
      String(referenceId)
    ) as EndPoint;

    return this.apiService.triggerApiRequest<any>(endpoint, HttpVerb.GET).pipe(
      map(res => this.extractFileId(res)),
      switchMap(fileId => (fileId ? of(fileId) : throwError(() => new Error('no-file-id')))),
      catchError(err => {
        // Distinguish ticket vs comment for user message by caller context; generic here
        const isTicket = referenceId === this.ticketId;
        const key = isTicket ? 'TICKET_DETAILS.NO_ATTACHMENT' : 'TICKET_DETAILS.NO_ATTACHMENT_COMMENT';
        // Only show toast if original error was not the intentional 'no-file-id' from mapping?
        // Always inform user when reference has no attachment
        this.toastr.error(this.translate.instant(key));
        return throwError(() => err);
      })
    );
  }

  private checkTicketAttachments(ticketId: number): void {
    if (!ticketId && ticketId !== 0) {
      this.hasTicketAttachments = false;
      return;
    }
    const endpoint = (EndPoint.GET_ATTACHMENT_BY_REFERENCE as string).replace(
      '{referenceId}',
      String(ticketId)
    ) as EndPoint;

    this.apiService
      .triggerApiRequest<any>(endpoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          // BaseResponse: { data: AttachmentDTO[] } — success means attachments exist
          const data = (res as any)?.data ?? (res as any)?.Data ?? res;
          if (Array.isArray(data)) {
            this.hasTicketAttachments = data.length > 0;
          } else if (data && typeof data === 'object') {
            // single attachment object -> has attachment
            this.hasTicketAttachments = true;
          } else {
            this.hasTicketAttachments = false;
          }
        },
        error: () => {
          // 404 NoDataFound from FetchAttachmentsOfReference -> no attachments
          this.hasTicketAttachments = false;
        },
      });
  }
}
