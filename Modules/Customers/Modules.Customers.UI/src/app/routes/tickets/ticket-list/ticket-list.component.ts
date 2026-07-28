import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { MatIcon } from '@angular/material/icon';
import { HeaderComponent } from '@theme/header/header.component';
import { PageHeaderComponent } from '@shared';
import { CommonModule, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateService } from '@ngx-translate/core';
import {
  MatCard,
  MatCardContent,
  MatCardHeader,
  MatCardTitle,
  MatCardSubtitle,
  MatCardAvatar,
} from '@angular/material/card';
import { RouterLink } from '@angular/router';
import { environment } from '@env/environment';
import { MatButtonToggle, MatButtonToggleGroup } from '@angular/material/button-toggle';

interface Ticket {
  id: number;
  title: string;
  subject: string; // Backend uses 'subject' instead of 'title'
  createdAt: string;
  createdDate: string; // Backend uses 'createdDate'
  status: string | number; // Backend might return number for status
  statusName?: string; // For displaying status name if available
  severityId?: number;
  severityName?: string;
  description?: string;
  assignedTo?: string;
  assignedToName?: string;
  updatedDate?: string;
  lastUpdate?: string;
}

@Component({
  selector: 'app-ticket-list',
  standalone: true,
  templateUrl: './ticket-list.component.html',
  styleUrls: ['./ticket-list.component.scss'],
  imports: [
    TranslateModule,
    MatIcon,
    MatTableModule,
    MatIconModule,
    MatButtonModule,
    PageHeaderComponent,
    HeaderComponent,
    DatePipe,
    CommonModule,
    MatCard,
    MatCardContent,
    MatCardHeader,
    MatCardTitle,
    MatCardSubtitle,
    MatCardAvatar,
    RouterLink,
    MatButtonToggle,
    MatButtonToggleGroup,
  ],
})
export class TicketListComponent {
  private translate = inject(TranslateService);
  private apiService = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  currentLang: string = 'en-US';
  setLang(lang: string) {
    this.currentLang = lang;
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
  tickets: Ticket[] = [];
  displayedColumns: string[] = [
    'id',
    'status',
    'severity',
    'title',
    // 'description', // Removed description column
    'Date submitted',
    'assignedTo',
    'last update',
    'action',
  ];
  customerId: number = 1;
  noTickets = false;
  isLoading = false;
  errorMessage = '';

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang') || 'en-US';
    this.currentLang = savedLang;
    this.translate.setDefaultLang('en-US');
    this.translate.use(savedLang);
    this.route.paramMap.subscribe(params => {
      const id = params.get('customerId');

      if (id) {
        this.customerId = +id;
        try {
          localStorage.setItem('customerId', String(this.customerId));
        } catch {}
      } else {
        // Fallback to stored value when route doesn't provide customer id
        const stored = Number(localStorage.getItem('customerId'));
        if (!Number.isNaN(stored) && stored > 0) {
          this.customerId = stored;
        }
      }
      // Always load tickets, using default customerId if none provided
      this.getTickets();
    });

    // Handle refresh parameter from query params
    this.route.queryParams.subscribe(params => {
      if (params.refresh === 'true') {
        this.getTickets();
        // Remove the refresh parameter from the URL
        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: { refresh: null },
          queryParamsHandling: 'merge',
        });
      }
    });
  }
  forceRefresh(): void {
    this.getTickets(true);
  }

  getTickets(forceRefresh: boolean = false): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.noTickets = false;

    // Get the base endpoint URL
    let endpointUrl = EndPoint.GET_TICKETS_BY_CUSTOMER_ID.replace(
      '{customerId}',
      this.customerId.toString()
    );

    // Add cache busting parameter if force refresh is requested
    if (forceRefresh) {
      const timestamp = new Date().getTime();
      endpointUrl += `${endpointUrl.includes('?') ? '&' : '?'}_=${timestamp}`;
    }

    // Cast to EndPoint enum for type safety
    const endpoint = endpointUrl as EndPoint;

    // Log the full request URL for debugging

    this.apiService
      .triggerApiRequest<any>(endpoint, HttpVerb.GET)
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe({
        next: (response: any) => {
          // Check if response is an array or has a data property
          const ticketsData = Array.isArray(response)
            ? response
            : response.data
              ? response.data
              : response.items || response.tickets || [];

          if (!Array.isArray(ticketsData)) {
            this.errorMessage = 'Unexpected data format received from server';
            return;
          }

          this.isLoading = false;
          // Map the backend response to our Ticket interface
          this.tickets = ticketsData.map((ticket: any) => ({
            id: ticket.id,
            title: ticket.subject || ticket.title, // Use subject as title if available
            subject: ticket.subject,
            createdAt: ticket.createdDate || ticket.createdAt,
            createdDate: ticket.createdDate,
            status: ticket.status,
            statusName: this.getStatusName(ticket.status),
            // API returns severity as a resolved string name (e.g. "Medium"), not just an ID.
            // Also capture the numeric ID for reference using the backend typo 'severtyId'.
            severityId: ticket.severtyId ?? ticket.severityId ?? null,
            severityName: this.resolveSeverityName(ticket),
            description: ticket.description,
            assignedTo: 'Unassigned', // Always set to 'Unassigned' regardless of API response
            updatedDate: ticket.updatedDate || ticket.createdDate || ticket.createdAt,
            lastUpdate: ticket.updatedDate || ticket.createdDate || ticket.createdAt,
          }));
          this.noTickets = ticketsData.length === 0;
          this.errorMessage = '';

          // Log detailed ticket information

          ticketsData.forEach((ticket: any, index: number) => {});

          // Log the current time to help with debugging
        },
        error: err => {
          this.isLoading = false;
          this.noTickets = true;
          this.tickets = [];

          // Set user-friendly error message
          if (err.status === 500) {
            this.errorMessage =
              'Server error occurred while fetching tickets. Please check if the backend endpoint is properly implemented.';
          } else if (err.status === 404) {
            this.errorMessage =
              'Tickets endpoint not found. Please verify the API endpoint exists.';
          } else if (err.status === 0) {
            this.errorMessage =
              'Unable to connect to the server. Please check if the backend is running.';
          } else {
            this.errorMessage = `Error fetching tickets: ${err.message || 'Unknown error'}`;
          }
        },
      });
  }

  viewDetails(ticket: Ticket): void {
    this.router.navigate(['/tickets', ticket.id, 'details']);
  }

  navigateToCreateTicket(): void {
    if (this.customerId) {
      this.router.navigate(['/tickets', this.customerId, 'create']);
    } else {
      this.router.navigate(['/tickets/create']);
    }
  }

  private getStatusName(status: number | string): string {
    const statusMap: { [key: number]: string } = {
      0: 'New',
      1: 'In Progress',
      2: 'Resolved',
      3: 'Closed',
      4: 'Cancelled',
    };

    if (typeof status === 'string') {
      return status; // Return as is if it's already a string
    }

    return statusMap[status] || `Status ${status}`;
  }

  private getSeverityName(severityId: number | null | undefined): string {
    if (severityId === null || severityId === undefined) return '';

    const severityMap: { [key: number]: string } = {
      1: 'Low',
      2: 'Medium',
      3: 'High',
      4: 'Critical',
    };

    return severityMap[severityId] || '';
  }

  /**
   * Resolve severity display name for a raw ticket object from the list API.
   * Priority:
   * 1. `ticket.severity`     — string already resolved by the backend (e.g. "Medium")
   * 2. `ticket.severityName` — alternate string field some API versions may use
   * 3. Numeric ID fallback   — handles `severtyId` (typo) or `severityId`
   */
  private resolveSeverityName(ticket: any): string {
    // Prefer the pre-resolved string name the API already returns
    if (ticket.severity && typeof ticket.severity === 'string') {
      return ticket.severity;
    }
    if (ticket.severityName && typeof ticket.severityName === 'string') {
      return ticket.severityName;
    }
    // Fall back to mapping numeric ID (handles backend typo)
    const id = ticket.severtyId ?? ticket.severityId ?? null;
    return this.getSeverityName(id);
  }
}
