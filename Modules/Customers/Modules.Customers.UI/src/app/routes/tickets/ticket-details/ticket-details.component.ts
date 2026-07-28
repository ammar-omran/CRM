import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { CommonModule } from '@angular/common';
import { TicketDetails, TicketHistory } from '@shared/interfaces/ticket.model';
import { PageHeaderComponent } from '@shared';
import { MatTableModule } from '@angular/material/table';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { MatCardContent, MatCard, MatCardTitle } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { MatDivider } from '@angular/material/divider';
import { MatButtonModule } from '@angular/material/button';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { catchError, forkJoin, map, of, switchMap, throwError } from 'rxjs';
import { TicketHistoryComponent } from '../ticket-history/ticket-history.component';
import { MatTab, MatTabGroup } from '@angular/material/tabs';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { RouterLink } from '@angular/router';
import { CommentComponent } from '@shared/utils/comment/comment.component';
import { AddCommentComponent } from '../add-comment/add-comment.component';
import { TranslateModule } from '@ngx-translate/core';
import { ToastrService } from 'ngx-toastr';
import { TokenService } from '@core/authentication/token.service';
import { MatButtonToggleGroup, MatButtonToggle } from '@angular/material/button-toggle';
import { TranslateService } from '@ngx-translate/core';
import { MatCardModule } from '@angular/material/card';

@Component({
  standalone: true,
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss'],
  imports: [
    CommonModule,
    PageHeaderComponent,
    MatTableModule,
    MatCardContent,
    MatCard,
    MatCardTitle,
    MatIcon,
    MatDivider,
    TicketHistoryComponent,
    MatTab,
    MatTabGroup,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    RouterLink,
    CommentComponent,
    AddCommentComponent,
    TranslateModule,
    MatButtonToggleGroup,
    MatButtonToggle,
    MatButtonModule,
    MatCardModule,
  ],
})
export class TicketDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private apiService = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  private translate: TranslateService = inject(TranslateService);
  private toastr = inject(ToastrService);
  private tokenService = inject(TokenService);
  ticketId!: number;
  hasTicketAttachments = false;
  currentLang: string = 'en-US';

  setLang(lang: string) {
    this.currentLang = lang;
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
  showHistoryTab = false;
  //showHistoryTab = true;
  ticket: TicketDetails | null = null;
  tickethistory: TicketHistory[] = [];
  displayedColumns: string[] = ['title', 'description', 'changedByName', 'changeDate'];
  displayedColumnsHistory: string[] = ['title', 'description', 'changedByName', 'changeDate'];
  error: string | null = null;
  notFound = false;

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang') || 'en-US';
    this.currentLang = savedLang;
    this.translate.setDefaultLang('en-US');
    this.translate.use(savedLang);
    const id = this.route.snapshot.paramMap.get('ticketId');
    if (id) {
      this.ticketId = +id;
      this.loadTicketDetails();
      this.getComments();
    } else {
      this.notFound = true;
    }
  }

  loadTicketDetails(): void {
    const endpoint = EndPoint.GET_TICKET_DETAILS.replace(
      '{ticketId}',
      this.ticketId.toString()
    ) as EndPoint;

    this.apiService
      .triggerApiRequest<TicketDetails>(endpoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.ticket = res;
          this.checkTicketAttachments(this.ticketId);
        },
        error: err => {
          this.notFound = true;
        },
      });
  }

  loadTicketHistory(): void {
    const endpoint = EndPoint.GET_TICKET_HISTORY.replace(
      '{ticketId}',
      this.ticketId.toString()
    ) as EndPoint;

    this.apiService
      .triggerApiRequest<TicketHistory[]>(endpoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          this.tickethistory = res;
        },
        error: err => { },
      });
  }
  onTabChange(event: any): void {
    if (event.index === 1) {
      this.showHistoryTab = true;
    }
  }
  ticketComments: any[] = [];
  // Attachment preview URL (object URL) for the current ticket
  attachmentUrl: string | null = null;
  customerId: string = localStorage.getItem('customerId') || 'no';
  getComments() {
    this.apiService
      .triggerApiRequest(
        `${EndPoint.GET_TICKET_COMMENTS}/${this.ticketId}` as EndPoint,
        HttpVerb.GET
      )
      .subscribe((response: any) => {
        if (response.status) {
          if (response.ticketComments != null) {
            this.ticketComments = response.ticketComments.map((comment: any) => {

              if (comment.createdById > 0) {
                return {
                  id: comment.id ?? comment.commentId ?? comment.referenceId,
                  author: comment.createdByName,
                  description: comment.description,
                  createdDate: comment.createdDate,
                  role: 'Support',
                  HaveAttachments: comment.haveAttachments,
                };
              } else {
                return {
                  id: comment.id ?? comment.commentId ?? comment.referenceId,
                  author: comment.createdByName,
                  description: comment.description,
                  createdDate: comment.createdDate,
                  role: 'Customer',
                  HaveAttachments: comment.haveAttachments,
                };
              }
            });

            // Main ticket-level buttons use Attachments/reference/{ticketId}.
            // Comment attachments use Attachments/reference/{commentId}.
            // Do NOT enable main buttons based on comment attachments (would cause 404).
            // For comments: if API does not return HaveAttachments, verify via Attachments API.
            this.enrichCommentsWithAttachmentStatus();
          }
        }
      });
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
          if (Array.isArray(res) && res.length > 0) return true;
          if (res && typeof res === 'object') {
            const data = (res as any).data;
            return Array.isArray(data) ? data.length > 0 : true;
          }
          return res != null;
        }),
        catchError(() => of(false))
      );
    });

    forkJoin(checks)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe(results => {
        results.forEach((has, i) => {
          this.ticketComments[i].HaveAttachments = this.ticketComments[i].HaveAttachments ?? has;
        });
      });
  }

  openAttachmentInNewTab(): void {
    if (!this.ticketId && this.ticketId !== 0) {
      this.toastr.error('Invalid ticket reference');
      return;
    }
    if (!this.hasTicketAttachments) {
      this.toastr.error(
        this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT')
      );
      return;
    }
    this.fetchTicketFileId(this.ticketId)
      .pipe(
        switchMap(fileId => {
          // Try fileId-based download first, then fallback to ticket direct download
          const byId = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          const byTicket = EndPoint.DOWNLOAD_TICKET_ATTACHMENT.replace(
            '{ticketId}',
            String(this.ticketId)
          ) as EndPoint;
          return this.apiService
            .triggerApiRequest(byId, HttpVerb.GET, undefined, undefined, {
              responseType: 'blob',
              observe: 'response',
              headers: { Accept: '*/*' },
            })
            .pipe(
              catchError(() =>
                this.apiService.triggerApiRequest(byTicket, HttpVerb.GET, undefined, undefined, {
                  responseType: 'blob',
                  observe: 'response',
                  headers: { Accept: '*/*' },
                })
              )
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res ?.body instanceof Blob ? res.body : new Blob([res ?.body || '']);
        const url = URL.createObjectURL(fileBlob);
        if (fileBlob.type && String(fileBlob.type).startsWith('image/')) {
          this.attachmentUrl = url;
        }
        window.open(url, '_blank');
      });
  }

  private resolveCommentId(comment: any): number | null {
    const candidate =
      comment ?.id ??
        comment ?.commentId ??
          comment ?.ticketCommentId ??
            comment ?.referenceId ??
              comment ?.comment_id;
    const numeric = candidate != null ? Number(candidate) : NaN;
    return Number.isFinite(numeric) ? numeric : null;
  }

  viewCommentAttachment(comment: any): void {
    const commentId = this.resolveCommentId(comment);
    if (commentId == null) {
      this.toastr.error('Invalid comment reference');
      return;
    }
    this.fetchCommentFileId(commentId)
      .pipe(
        switchMap(fileId => {
          const byId = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          const byComment = (EndPoint.DOWNLOAD_COMMENT_ATTACHMENT as string).replace(
            '{commentId}',
            String(commentId)
          ) as EndPoint;
          return this.apiService
            .triggerApiRequest(byId, HttpVerb.GET, undefined, undefined, {
              responseType: 'blob',
              observe: 'response',
              headers: { Accept: '*/*' },
            })
            .pipe(
              catchError(() =>
                this.apiService.triggerApiRequest(byComment, HttpVerb.GET, undefined, undefined, {
                  responseType: 'blob',
                  observe: 'response',
                  headers: { Accept: '*/*' },
                })
              )
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res ?.body instanceof Blob ? res.body : new Blob([res ?.body || '']);
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
    this.fetchCommentFileId(commentId)
      .pipe(
        switchMap(fileId => {
          const byId = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          const byComment = (EndPoint.DOWNLOAD_COMMENT_ATTACHMENT as string).replace(
            '{commentId}',
            String(commentId)
          ) as EndPoint;
          return this.apiService
            .triggerApiRequest(byId, HttpVerb.GET, undefined, undefined, {
              responseType: 'blob',
              observe: 'response',
              headers: { Accept: '*/*' },
            })
            .pipe(
              catchError(() =>
                this.apiService.triggerApiRequest(byComment, HttpVerb.GET, undefined, undefined, {
                  responseType: 'blob',
                  observe: 'response',
                  headers: { Accept: '*/*' },
                })
              )
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res ?.body instanceof Blob ? res.body : new Blob([res ?.body || '']);
        const contentDisposition: string = res ?.headers ?.get ?.('content-disposition') || '';
        const filenameMatch = /filename\*=UTF-8''([^;]+)|filename=([^;]+)/i.exec(
          contentDisposition
        );
        const filename = decodeURIComponent(
          filenameMatch ?.[1] || filenameMatch ?.[2] || `comment-${commentId}`
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
      this.toastr.error(
        this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT')
      );
      return;
    }
    this.fetchTicketFileId(this.ticketId)
      .pipe(
        switchMap(fileId => {
          const byId = EndPoint.DOWNLOAD_ATTACHMENT_BY_ID.replace(
            '{fileId}',
            String(fileId)
          ) as EndPoint;
          const byTicket = EndPoint.DOWNLOAD_TICKET_ATTACHMENT.replace(
            '{ticketId}',
            String(this.ticketId)
          ) as EndPoint;
          return this.apiService
            .triggerApiRequest(byId, HttpVerb.GET, undefined, undefined, {
              responseType: 'blob',
              observe: 'response',
              headers: { Accept: '*/*' },
            })
            .pipe(
              catchError(() =>
                this.apiService.triggerApiRequest(byTicket, HttpVerb.GET, undefined, undefined, {
                  responseType: 'blob',
                  observe: 'response',
                  headers: { Accept: '*/*' },
                })
              )
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe((res: any) => {
        const fileBlob = res ?.body instanceof Blob ? res.body : new Blob([res ?.body || '']);
        const contentDisposition: string = res ?.headers ?.get ?.('content-disposition') || '';
        const filenameMatch = /filename\*=UTF-8''([^;]+)|filename=([^;]+)/i.exec(
          contentDisposition
        );
        const filename = decodeURIComponent(
          filenameMatch ?.[1] || filenameMatch ?.[2] || `ticket-${this.ticketId}`
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

  private extractFileId(refRes: any): number | null {
    if (!refRes) return null;
    if (typeof refRes === 'object' && 'fileId' in refRes) return Number(refRes.fileId);
    if (typeof refRes === 'object' && 'id' in refRes) return Number(refRes.id);
    if (Array.isArray(refRes) && refRes.length > 0) {
      const first = refRes[0];
      if (first ?.fileId) return Number(first.fileId);
      if (first ?.id) return Number(first.id);
    }
    if (refRes ?.data) return this.extractFileId(refRes.data);
    return null;
  }

  private tryEndpointsForFileId(attempts: Array<{ endpoint: string; params?: any }>): any {
    if (!attempts || attempts.length === 0) {
      return throwError(() => new Error('no-attempts'));
    }
    const [{ endpoint, params }, ...rest] = attempts;
    // Debug the attempted endpoint for troubleshooting 404s
    // eslint-disable-next-line no-console
    console.debug('Attachment reference attempt', endpoint, params || {});
    return this.apiService.triggerApiRequest<any>(endpoint as EndPoint, HttpVerb.GET, params).pipe(
      map(res => this.extractFileId(res)),
      switchMap(fileId => (fileId ? of(fileId) : throwError(() => new Error('no-file-id')))),
      catchError(() =>
        rest.length ? this.tryEndpointsForFileId(rest) : throwError(() => new Error('no-file-id'))
      )
    );
  }

  private fetchTicketFileId(ticketId: number) {
    const attempts = [
      {
        endpoint: (EndPoint.GET_ATTACHMENT_BY_REFERENCE as string).replace(
          '{referenceId}',
          String(ticketId)
        ),
      },
      {
        endpoint: (EndPoint.GET_ATTACHMENT_BY_TICKET_REFERENCE as string).replace(
          '{ticketId}',
          String(ticketId)
        ),
      },
      { endpoint: 'Attachments/reference', params: { ticketId } },
      { endpoint: 'Attachments/reference', params: { referenceId: ticketId } },
      { endpoint: `Attachments/reference/ticketid/${ticketId}` },
    ];
    return this.tryEndpointsForFileId(attempts).pipe(
      catchError(err => {
        this.toastr.error(this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT'));
        return throwError(() => err);
      })
    );
  }

  private fetchCommentFileId(commentId: number) {
    const attempts = [
      {
        endpoint: (EndPoint.GET_ATTACHMENT_BY_REFERENCE as string).replace(
          '{referenceId}',
          String(commentId)
        ),
      },
      {
        endpoint: (EndPoint.GET_ATTACHMENT_BY_COMMENT_REFERENCE as string).replace(
          '{commentId}',
          String(commentId)
        ),
      },
      { endpoint: 'Attachments/reference', params: { commentId } },
      { endpoint: 'Attachments/reference/commentid/' + commentId },
      { endpoint: 'Attachments/reference', params: { referenceId: commentId } },
    ];
    return this.tryEndpointsForFileId(attempts).pipe(
      catchError(err => {
        this.toastr.error(this.translate.instant('TICKET_DETAILS.NO_ATTACHMENT_COMMENT'));
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
          let has = false;
          if (Array.isArray(res)) {
            has = res.length > 0;
          } else if (res && typeof res === 'object') {
            if (Array.isArray((res as any).data)) {
              has = (res as any).data.length > 0;
            } else {
              has = true;
            }
          } else if (res != null) {
            has = true;
          }
          this.hasTicketAttachments = has;
        },
        error: () => {
          this.hasTicketAttachments = false;
        },
      });
  }
}
