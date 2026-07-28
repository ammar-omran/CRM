import { Component, Input } from '@angular/core';
import { TicketHistory } from '@shared/interfaces/ticket-history';
import { MatCard } from '@angular/material/card';
import { MatIcon } from '@angular/material/icon';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-ticket-history-record',
  templateUrl: './ticket-history-record.component.html',
  styleUrls: ['./ticket-history-record.component.scss'],
  standalone: true,
  imports: [MatCard, MatIcon, DatePipe],
})
export class TicketHistoryRecordComponent {
  @Input() record!: TicketHistory;
}
