import { Component, inject, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { TicketHistory } from '@shared/interfaces/ticket.model';
import { HelperService } from '@shared/services/helper.service';
import { TicketHistoryRecordComponent } from './ticket-history-record/ticket-history-record.component';
import { MatProgressSpinner } from '@angular/material/progress-spinner';
import { CommonModule } from '@angular/common';
import { BaseResponse } from '@shared/interfaces/base-response';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

@Component({
  selector: 'app-ticket-history',
  standalone: true,
  templateUrl: './ticket-history.component.html',
  imports: [CommonModule, TicketHistoryRecordComponent, MatProgressSpinner],
})
export class TicketHistoryComponent implements OnChanges {
  @Input() ticketId!: number;
  private apiService = inject(ApiService);

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
      .triggerApiRequest<BaseResponse<TicketHistory[]>>(endpoint as EndPoint, HttpVerb.GET)
      .subscribe({
        next: res => {
          if (res.status.code === ResponseStatusEnum.Success) {
            this.history = res.data;
          } else {
            this.error = res.status.message;
          }
          this.loading = false;
        },
        error: err => {
          this.error = 'Failed to fetch history.';
          this.loading = false;
        },
      });
  }
}
