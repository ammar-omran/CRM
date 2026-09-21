import { Component, DestroyRef, inject } from '@angular/core';
import { FilterComponent } from '../../shared/components/filter/filter.component';
import { AdaptiveTableComponent } from '../../shared/components/adaptive-table/adaptive-table.component';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { MtxGridColumn } from '@ng-matero/extensions/grid';
import { RetiredMeter } from '@shared/interfaces/retired-meter.model';
import { PageHeaderComponent } from '@shared';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { downloadFile, FileFormats, getFileExtension } from '@shared/utils/file-utils';
import { HttpResponse } from '@angular/common/http';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { map } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { ApiService } from '@shared/services/api.service';

@Component({
  selector: 'app-retired-meters',
  standalone: true,
  imports: [
    FilterComponent,
    AdaptiveTableComponent,
    PageHeaderComponent,
    ListActionsComponent,
    ListActionsComponent,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './retired-meters.component.html',
  styleUrl: './retired-meters.component.scss',
})
export class RetiredMetersComponent {
  filterVisible = false;
  fileFormats = FileFormats;
  private apiService = inject(ApiService);
  private destroyRef = inject(DestroyRef);
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }

  filters: any = {};
  columns: MtxGridColumn<RetiredMeter>[] = [
    { header: 'Meter Serial', field: 'meterSerial', sortable: true },
    { header: 'Status', field: 'status', sortable: true },
    { header: 'Meter model', field: 'meterModel', sortable: true },
    { header: 'Meter type', field: 'type', sortable: true },
    { header: 'Meter make', field: 'meterMake', sortable: true },
    { header: 'Retired By', field: 'agentName', sortable: true },
  ];
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
      formControlName: 'MeterSerial',
      label: 'MeterSerial',
      type: 'text',
      optionLabel: 'meterSerial',
      optionVal: 'MeterSerial',
    },
    {
      formControlName: 'MeterTypeId',
      label: 'Meter Type',
      type: 'select',
      apiEndpoint: EndPoint.METER_TYPES,
      optionLabel: 'name',
      optionVal: 'id',
    },
    {
      formControlName: 'MeterMakeId',
      label: 'Meter Make',
      type: 'select',
      apiEndpoint: EndPoint.METER_MAKES,
      optionLabel: 'name',
      optionVal: 'id',
    },
    {
      formControlName: 'contractorId',
      label: 'Contractor',
      type: 'select',
      apiEndpoint: EndPoint.CONTRACTORS,
      optionLabel: 'name',
      optionVal: 'id',
    },
    {
      formControlName: 'startDate',
      label: 'Start Date',
      type: 'date',
    },
    {
      formControlName: 'endDate',
      label: 'End Date',
      type: 'date',
    },
    {
      formControlName: 'agentId',
      label: 'Retired By',
      type: 'select',
      apiEndpoint: EndPoint.AGENTS_LIST,
      optionLabel: 'name',
      optionVal: 'id',
    },
  ];
  endpoint: EndPoint = EndPoint.RETIRED_METERS;
  httpVerb: HttpVerb = HttpVerb.GET;

  onFilterChanged(filterValues: any): void {
    const newFilters: any = { ...filterValues };

    if (filterValues?.startDate) {
      newFilters.startDate = new Date(filterValues.startDate).toISOString();
    } else {
      delete newFilters.startDate;
    }

    if (filterValues?.endDate) {
      newFilters.endDate = new Date(filterValues.endDate).toISOString();
    } else {
      delete newFilters.endDate;
    }

    this.filters = newFilters;
  }

  exportMeters(format: string): void {
    if (!format) return;

    const meterSerial = this.filters?.MeterSerial ?? '';
    const districtId = this.filters?.districtID ?? '';
    const fileType = format;

    this.apiService
      .triggerApiRequest(
        EndPoint.EXPORT_RETIRED_METERS,
        HttpVerb.GET,
        {
          fileType: format,
          meterSerial: encodeURIComponent(meterSerial),
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
          const defaultFilename = `meters_${new Date().toISOString().slice(0, 10)}.${extension}`;
          downloadFile(response, defaultFilename, format);
        },
        error: err => {
          console.error('Export failed:', err);
        },
      });
  }
}
