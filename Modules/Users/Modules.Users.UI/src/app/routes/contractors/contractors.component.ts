import { Component, DestroyRef, inject } from '@angular/core';
import { Router } from '@angular/router';
import { HttpResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { MtxGridColumn } from '@ng-matero/extensions/grid';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { Contractor } from '@shared/interfaces/contractor.model';
import { AdaptiveTableComponent } from '@shared/components/adaptive-table/adaptive-table.component';
import { FilterComponent } from '@shared/components/filter/filter.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatOptionModule } from '@angular/material/core';
import { MatInputModule } from '@angular/material/input';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ListActionsComponent } from '../../shared/components/list-actions/list-actions.component';
import { MatMenuModule } from '@angular/material/menu';
import { downloadFile, FileFormats, getFileExtension, getMimeType } from '@shared/utils/file-utils';
import { ApiService } from '@shared/services/api.service';
import { map } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-contractors',
  standalone: true,
  templateUrl: './contractors.component.html',
  styleUrls: ['./contractors.component.scss'],
  imports: [
    FormsModule,
    AdaptiveTableComponent,
    FilterComponent,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatSelectModule,
    MatOptionModule,
    MatInputModule,
    PageHeaderComponent,
    ListActionsComponent,
    MatMenuModule,
  ],
})
export class ContractorsComponent {
  filterVisible = false;
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }
  private router = inject(Router);
  private apiService = inject(ApiService);
  private destroyRef = inject(DestroyRef);

  filters: any = {};

  endpoint: EndPoint = EndPoint.GET_Contractors_BY_DISTRICT_ID;
  httpVerb: HttpVerb = HttpVerb.GET;

  fileFormats = FileFormats;

  filterControls: FilterControl[] = [
    {
      formControlName: 'districtID',
      label: 'District',
      type: 'select',
      apiEndpoint: EndPoint.DISTRICTS_LIST,
      optionLabel: 'name',
      optionVal: 'id',
    },
    {
      formControlName: 'Name',
      label: 'Search By Name',
      type: 'text',
    },
  ];

  columns: MtxGridColumn[] = [
    { header: 'Code', field: 'code', sortable: true },
    { header: 'Name', field: 'name', sortable: true },
    { header: 'Office Address', field: 'officeAddress' },
    { header: 'Ghana Post Address', field: 'ghanaPostAddress' },
    { header: 'Contact Person', field: 'contactPersonName' },
    { header: 'Phone', field: 'phone' },
    {
      header: 'Actions',
      field: 'action',
      width: '150px',
      pinned: 'right',
      right: '0px',
      type: 'button',
      buttons: [
        {
          type: 'icon',
          icon: 'visibility',
          tooltip: 'Show Agents',
          click: (row: Contractor) => this.viewAgents(row.id!),
        },
        {
          type: 'icon',
          icon: 'edit',
          tooltip: 'Edit Contractor',
          click: (row: Contractor) => this.editContractor(row.id!),
        },
      ],
    },
  ];

  onFilterChanged(filterValues: any): void {
    this.filters = filterValues;
  }

  viewAgents(contractorId: number): void {
    this.router.navigate([`contractors/${contractorId}/agents`]);
  }

  editContractor(contractorId: number): void {
    this.router.navigate([`contractors/edit/${contractorId}`]);
  }

  exportContractors(format: string): void {
    if (!format) return;

    const name = this.filters?.Name ?? '';
    const districtId = this.filters?.districtID ?? '';
    const fileType = format;

    this.apiService
      .triggerApiRequest(
        EndPoint.EXPORT_CONTRACTORS,
        HttpVerb.GET,
        {
          fileType: format,
          name: encodeURIComponent(name),
          districtId,
        },
        null,
        { responseType: 'blob', observe: 'response' }
      )
      .pipe(
        map(response => response as HttpResponse<Blob>),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe({
        next: response => {
          const extension = getFileExtension(format);
          const defaultFilename = `contractors_${new Date().toISOString().slice(0, 10)}.${extension}`;
          downloadFile(response, defaultFilename, format);
        },
        error: err => {
          console.error('Export failed:', err);
        },
      });
  }
}
