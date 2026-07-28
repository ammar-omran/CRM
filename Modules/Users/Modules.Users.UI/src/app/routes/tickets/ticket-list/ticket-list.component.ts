import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { EndPoint, HttpVerb } from '@shared/enums';
import { PageHeaderComponent } from '@shared';
import { CommonModule, DatePipe } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { TranslateModule } from '@ngx-translate/core';
import { FilterComponent } from '@shared/components/filter/filter.component';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { AdaptiveTableComponent } from '@shared/components/adaptive-table/adaptive-table.component';
import { MtxGridColumn, MtxGridColumnButton } from '@ng-matero/extensions/grid';
import { LookupService } from '@shared/services/lookup.service';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

interface AdminTicket {
  id: number;
  title: string;
  status: string;
  createdAt: string;
  severity: string;
  assignedTo: string;
  updatedByName?: string;
  lastUpdate: string;
}

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss'],
  imports: [
    TranslateModule,
    MatIconModule,
    MatButtonModule,
    PageHeaderComponent,
    CommonModule,
    FilterComponent,
    ListActionsComponent,
    AdaptiveTableComponent,
  ],
  providers: [DatePipe],
})
export class TicketListComponent implements OnInit {
  private router = inject(Router);
  private datePipe = inject(DatePipe);
  private lookupService = inject(LookupService);
  filterVisible = false;
  filters: any = {};
  isFilterApplied = false;

  endpoint: EndPoint = EndPoint.GET_TICKETS;
  httpVerb: HttpVerb = HttpVerb.GET;

  columns: MtxGridColumn<AdminTicket>[] = [
    { header: 'Reference', field: 'id', sortable: true },
    { header: 'Title', field: 'title' },
    { header: 'Status', field: 'status' },
    {
      header: 'Created Date',
      field: 'createdAt',
      sortable: true,
      width: '10%',
      formatter: (data: AdminTicket) =>
        data.createdAt ? this.datePipe.transform(data.createdAt, 'medium') ?? '' : '',
    },
    {
      header: 'Updated',
      field: 'lastUpdate',
      sortable: true,
      width: '10%',
      formatter: (data: AdminTicket) =>
        data.lastUpdate ? this.datePipe.transform(data.lastUpdate, 'medium') ?? '' : '',
    },
    {
      header: 'Assigned To',
      field: 'assignedTo',
      formatter: (data: AdminTicket) => data.updatedByName || data.assignedTo || 'Unassigned',
    },
    { header: 'Severity', field: 'severity' },
    {
      header: 'Actions',
      field: 'action' as keyof AdminTicket,
      width: '160px',
      pinned: 'right',
      right: '0px',
      type: 'button',
      buttons: (row: AdminTicket): MtxGridColumnButton<AdminTicket>[] => [
        {
          type: 'icon',
          text: 'Details',
          icon: 'arrow_forward',
          tooltip: 'Details',
          click: () => this.viewDetails(row),
        },
      ],
    },
  ];

  filterControls: FilterControl[] = [
    {
      formControlName: 'fromDate',
      label: 'From Date',
      type: 'date',
    },
    {
      formControlName: 'toDate',
      label: 'To Date',
      type: 'date',
    },
    {
      formControlName: 'severity',
      label: 'Severity',
      type: 'select',
      canSelectMulti: true,
    },
    {
      formControlName: 'status',
      label: 'Status',
      type: 'select',
      canSelectMulti: true,
    },
    {
      formControlName: 'supportLine',
      label: 'Support Line',
      type: 'select',
      canSelectMulti: true,
    },
  ];

  ngOnInit(): void {
    this.filterControls
      .filter(e => e.type == 'select')
      .forEach(e => {
        this.lookupService.getTicketFilters(e.formControlName).subscribe({
          next: response => {
            if (response.status.code == ResponseStatusEnum.Success) {
              e.options = response.data.map(item => ({
                value: item.id,
                label: item.name,
              }));
            } else {
              e.options = [];
            }
          },
          error: () => (e.options = []),
        });
      });
  }
  noTickets = false;

  toggleFilter(): void {
    this.filterVisible = !this.filterVisible;
  }

  onFilterChanged(filterValues: any): void {
    const transformedFilters: any = {};
    console.log(transformedFilters);

    if (filterValues.severity) {
      transformedFilters.SeverityIds = filterValues.severity;
    }

    if (filterValues.status) {
      transformedFilters.StatusIds = filterValues.status;
    }

    if (filterValues.supportLine) {
      transformedFilters.SupportLineNums = filterValues.supportLine;
    }

    if (filterValues.fromDate) {
      const fromDate = new Date(filterValues.fromDate);
      if (!isNaN(fromDate.getTime())) {
        transformedFilters.FromDate = this.datePipe.transform(fromDate, 'yyyy-MM-ddTHH:mm:ss');
      }
    }

    if (filterValues.toDate) {
      const toDate = new Date(filterValues.toDate);
      if (!isNaN(toDate.getTime())) {
        transformedFilters.ToDate = this.datePipe.transform(toDate, 'yyyy-MM-ddTHH:mm:ss');
      }
    }

    this.filters = transformedFilters;
    this.isFilterApplied = Object.keys(this.filters).length > 0;
  }

  viewDetails(ticket: AdminTicket): void {
    this.router.navigate(['/tickets', ticket.id, 'details']);
  }
}
