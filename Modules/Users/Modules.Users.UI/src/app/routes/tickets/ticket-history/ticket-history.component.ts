import { Component, DestroyRef, inject, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { TicketHistory } from '@shared/interfaces/ticket-history';
import { HelperService } from '@shared/services/helper.service';
import { TicketHistoryRecordComponent } from './ticket-history-record/ticket-history-record.component';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-ticket-history',
  standalone: true,
  templateUrl: './ticket-history.component.html',
  imports: [CommonModule, TicketHistoryRecordComponent, MatProgressSpinner],
})
export class TicketHistoryComponent implements OnChanges {
  @Input() ticketId!: number;
  private readonly apiService = inject(ApiService);
  private readonly destroyRef = inject(DestroyRef);

  loading = false;
  error: string | null = null;
  history: TicketHistory[] = [];

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.ticketId && this.ticketId) {
      this.fetchTicketHistory();
    }
  }

  private fetchTicketHistory(): void {
    this.loading = true;
    this.error = null;

    const endpoint = HelperService.formatEndpoint(EndPoint.GET_TICKET_HISTORY, {
      ticketId: this.ticketId,
    });

    this.apiService
      .triggerApiRequest<any>(endpoint as EndPoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: res => {
          // New backend: returns TicketHistory[] directly (Ok(result.Value))
          // Legacy: BaseResponse { data, status }
          const data = res?.data ?? res?.Data ?? res;
          if (Array.isArray(data)) {
            this.history = data;
          } else if (res?.status?.code === 0 || res?.Status?.Code === 0) {
            this.history = res.data ?? [];
          } else {
            this.history = [];
          }
          this.loading = false;
        },
        error: () => {
          this.error = 'Failed to fetch history.';
          this.loading = false;
        },
      });
  }
}
