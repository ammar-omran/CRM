import { Component, inject } from '@angular/core';
import { MtxGridColumn } from '@ng-matero/extensions/grid';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { AdaptiveTableComponent } from '@shared/components/adaptive-table/adaptive-table.component';
import { FilterComponent } from '@shared/components/filter/filter.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { DatePipe } from '@angular/common';
import { ApiService } from '@shared/services/api.service';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { PageHeaderComponent } from '@shared';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { downloadFile, FileFormats, getFileExtension } from '@shared/utils/file-utils';
import { HttpResponse } from '@angular/common/http';
import { map } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DestroyRef } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-audits',
  standalone: true,
  imports: [
    AdaptiveTableComponent,
    FilterComponent,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    MatFormFieldModule,
    MatSelectModule,
    PageHeaderComponent,
    ListActionsComponent,
    MatMenuModule,
  ],
  templateUrl: './audits.component.html',
  styleUrls: ['./audits.component.scss'],
  providers: [DatePipe],
})
export class AuditsComponent {
  filterVisible = false;
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }

  private destroyRef = inject(DestroyRef);
  private toastr = inject(ToastrService);

  filters: any = {};
  endpoint: EndPoint = EndPoint.GET_AUDITS;
  httpVerb: HttpVerb = HttpVerb.GET;

  fileFormats = FileFormats;

  constructor(
    private datePipe: DatePipe,
    private apiService: ApiService
  ) {}

  columns: MtxGridColumn[] = [
    { header: 'Field', field: 'field' },
    { header: 'Old Value', field: 'oldValue' },
    { header: 'New Value', field: 'newValue' },
    { header: 'Operation', field: 'operation' },
    { header: 'UserName', field: 'userName' },
    { header: 'UserId', field: 'userId' },
    {
      header: 'Creation Date',
      field: 'creationDate',
      formatter: (data: any) => {
        if (!data.creationDate) return '';
        return this.datePipe.transform(data.creationDate, 'MMM d, yyyy h:mm a');
      },
    },
    { header: 'IP Address', field: 'ipAddress' },
    { header: 'Machine Name', field: 'machineName' },
  ];

  filterControls: FilterControl[] = [
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
      formControlName: 'userSearch',
      label: 'User',
      type: 'autocomplete',
      isFirstValueDynamic: true,
      apiEndpoint: EndPoint.Get_USERS,
    },
  ];

  onFilterChanged(filterValues: any): void {
    const startDateValid =
      filterValues.startDate && !isNaN(new Date(filterValues.startDate).getTime());
    const endDateValid = filterValues.endDate && !isNaN(new Date(filterValues.endDate).getTime());

    const transformedFilters: any = { ...filterValues };

    if (transformedFilters.userSearch) {
      transformedFilters.userId = transformedFilters.userSearch;
      delete transformedFilters.userSearch;
    }

    if (startDateValid) {
      transformedFilters.startDate = this.datePipe.transform(
        filterValues.startDate,
        'yyyy-MM-ddTHH:mm:ss'
      );
    } else {
      delete transformedFilters.startDate;
    }

    if (endDateValid) {
      transformedFilters.endDate = this.datePipe.transform(
        filterValues.endDate,
        'yyyy-MM-ddTHH:mm:ss'
      );
    } else {
      delete transformedFilters.endDate;
    }

    this.filters = transformedFilters;
  }

  export(format: string): void {
    if (!format) return;

    this.apiService
      .triggerApiRequest(
        EndPoint.GET_AUDITS,
        HttpVerb.GET,
        { ...this.filters, export: format },
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
          const defaultFilename = `audits_${new Date().toISOString().slice(0, 10)}.${extension}`;
          downloadFile(response, defaultFilename, format);
        },
        error: err => {
          if (err.error instanceof Blob && err.error.type === 'application/json') {
            err.error
              .text()
              .then((text: string) => {
                try {
                  const json = JSON.parse(text);
                  this.toastr.error(json.message || 'Export failed.');
                } catch {
                  this.toastr.error('Export failed. Unexpected error format.');
                }
              })
              .catch(() => {
                this.toastr.error('Export failed. Unexpected error format.');
              });
          } else {
            this.toastr.error('Export failed. Please check your filters.');
          }
        },
      });
  }
}
