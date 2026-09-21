import {
  ChangeDetectorRef,
  Component,
  inject,
  OnDestroy,
  OnInit,
  TemplateRef,
  ViewChild,
} from '@angular/core';
import { FilterComponent } from '../../../shared/components/filter/filter.component';
import { AdaptiveTableComponent } from '../../../shared/components/adaptive-table/adaptive-table.component';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { MtxGridColumn, MtxGridRowClassFormatter } from '@ng-matero/extensions/grid';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Agent } from '@shared/interfaces/agent.model';
import { map, Subscription } from 'rxjs';
import { MatTooltipModule } from '@angular/material/tooltip';
import { AdaptiveDialogComponent } from '@shared/components/adaptive-dialog/adaptive-dialog.component';
import { MatDialog } from '@angular/material/dialog';
import { HelperService } from '@shared/services/helper.service';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { BaseResponse } from '@shared/interfaces/base-response';
import { Contractor } from '@shared/interfaces/contractor.model';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { environment } from '@env/environment';
import { FormsModule } from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatMenuModule } from '@angular/material/menu';
import { PageHeaderComponent } from '@shared';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { downloadFile, FileFormats, getFileExtension, getMimeType } from '@shared/utils/file-utils';

@Component({
  selector: 'app-agents',
  standalone: true,
  imports: [
    FilterComponent,
    AdaptiveTableComponent,
    MatIconModule,
    MatButtonModule,
    RouterLink,
    MatTooltipModule,
    FormsModule,
    MatSelectModule,
    MatOptionModule,
    PageHeaderComponent,
    ListActionsComponent,
    MatMenuModule,
  ],
  templateUrl: './agents.component.html',
  styleUrl: './agents.component.scss',
})
export class AgentsComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private dialog = inject(MatDialog);
  private cdr = inject(ChangeDetectorRef);
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private http = inject(HttpClient);
  filterVisible = false;
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }

  contractorId!: string;
  contractorName?: string;
  filters: any = {};
  alive = false;
  selectedFormat: string | null = null;
  fileFormats = FileFormats;

  rowClassFormatter: MtxGridRowClassFormatter = {
    'disabled-state': (data, index) => data.state === 'disabled',
  };
  columns: MtxGridColumn<Agent>[] = [
    { header: 'Code', field: 'code', sortable: true },
    { header: 'Name', field: 'name', sortable: true },
    { header: 'Email', field: 'email', sortable: true },
    { header: 'Ghana Card', field: 'ghanaCard' },
    { header: 'Mobile', field: 'mobile' },
    {
      header: 'State',
      field: 'isActive',
      class: data => (data?.isActive ? 'text-success' : ''),
      sortable: true,
      formatter: (rowData: Agent) => (rowData.isActive ? ' Enabled' : 'Disabled'),
    },
    { header: 'Actions', field: 'actions' as keyof Agent, width: '170px' },
  ];
  filterControls: FilterControl[] = [
    {
      formControlName: 'Name',
      label: 'Search By Name',
      type: 'text',
      optionLabel: 'name',
      optionVal: 'Name',
    },
  ];
  httpVerb: HttpVerb = HttpVerb.GET;
  endpoint!: EndPoint;

  private routeSub!: Subscription;
  private viewDetailsSub!: Subscription;
  private contractorsSub!: Subscription;

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      this.contractorId = params.get('contractorId')!;
      if (this.contractorId) {
        const url = HelperService.formatEndpoint(EndPoint.GET_AGENTS_BY_CONTRACTOR_ID, {
          contractorId: this.contractorId,
        });
        this.endpoint = url as EndPoint;
        this.fetchContractorDetails();
      }
    });
  }

  ngOnDestroy(): void {
    this.routeSub?.unsubscribe();
    this.viewDetailsSub?.unsubscribe();
  }

  onFilterChanged(filterValues: any): void {
    this.filters = filterValues;
  }

  fetchContractorDetails(): void {
    this.contractorsSub = this.apiService
      .triggerApiRequest<BaseResponse<Contractor>>(EndPoint.GET_CONTRACTOR_BY_ID, HttpVerb.GET, {
        id: this.contractorId,
      })
      .subscribe({
        next: res => {
          this.contractorName = res.data.name;
        },
      });
  }

  toggleAgentState(agent: Agent): void {
    const dialogRef = this.dialog.open<AdaptiveDialogComponent>(AdaptiveDialogComponent, {
      width: '400px',
      data: {
        title: agent.isActive ? 'Disable Agent' : 'Enable Agent',
        message: `Are you sure you want to ${agent.isActive ? 'disable' : 'enable'} ${agent.name}?`,
        mode: 'confirmation',
      },
    });

    dialogRef.afterClosed().subscribe((confirmed?: boolean) => {
      if (confirmed === true) {
        // Update agent status
        const url = HelperService.formatEndpoint(EndPoint.UPDATE_AGENT_STATUS, {
          agentId: agent.id,
        });

        (this.routeSub = this.apiService
          .triggerApiRequest(url as EndPoint, HttpVerb.PUT)
          .subscribe({
            next: () => {
              this.filters = { ...this.filters };
              this.toastr.success('Agent updated successfully');
            },
            error: () => {
              this.toastr.error('Failed to update agent status');
            },
          })),
          (agent.state = agent.isActive ? 'disabled' : 'enabled');
      }
    });
  }

  exportAgents(format: string): void {
    if (!format) return;

    const name = this.filters?.Name ?? '';
    const fileType = format;

    this.apiService
      .triggerApiRequest(
        EndPoint.EXPORT_AGENTS,
        HttpVerb.GET,
        {
          fileType: format,
          contractorId: this.contractorId,
          name: encodeURIComponent(name),
        },
        null,
        { responseType: 'blob', observe: 'response' }
      )
      .pipe(map(response => response as HttpResponse<Blob>))
      .subscribe({
        next: response => {
          const extension = getFileExtension(format);
          const defaultFilename = `agents_${new Date().toISOString().slice(0, 10)}.${extension}`;
          downloadFile(response, defaultFilename, format);
        },
        error: err => {
          console.error('Export failed:', err);
        },
      });
  }
}
