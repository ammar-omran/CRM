import { Component, OnInit, OnDestroy, inject, DestroyRef } from '@angular/core';
import { FilterComponent } from '../../shared/components/filter/filter.component';
import { AdaptiveTableComponent } from '../../shared/components/adaptive-table/adaptive-table.component';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { MtxGridColumn, MtxGridColumnButton } from '@ng-matero/extensions/grid';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { AdaptiveDialogComponent } from '@shared/components/adaptive-dialog/adaptive-dialog.component';
import { ApiService } from '@shared/services/api.service';
import { of, Subscription, switchMap } from 'rxjs';
import { BaseResponse } from '@shared/interfaces/base-response';
import { Contractor } from '@shared/interfaces/contractor.model';
import { InstalledMeter } from '@shared/interfaces/Installed-meter.model';
import { MeterDetailsDialogComponent } from './meter-details-dialog/meter-details-dialog.component';
import { BaseMeter } from '@shared/interfaces/meter-info.model';
import { ToastrService } from 'ngx-toastr';
import { HelperService } from '@shared/services/helper.service';
import { PageHeaderComponent } from '../../shared/components/page-header/page-header.component';
import { ListActionsComponent } from '../../shared/components/list-actions/list-actions.component';
import { MatIconModule } from '@angular/material/icon';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { downloadFile, FileFormats, getFileExtension, getMimeType } from '@shared/utils/file-utils';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-installations',
  standalone: true,
  imports: [
    FilterComponent,
    AdaptiveTableComponent,
    MatDialogModule,
    PageHeaderComponent,
    ListActionsComponent,
    MatIconModule,
    MatMenuModule,
  ],
  templateUrl: './installations.component.html',
  styleUrl: './installations.component.scss',
})
export class InstallationsComponent implements OnInit, OnDestroy {
  filterVisible = false;
  filters: any = {};
  sortField: string = 'meterSerial';
  sortDirection: 'asc' | 'desc' = 'asc';
  fileFormats = FileFormats;
  private destroyRef = inject(DestroyRef);
  columns: MtxGridColumn<InstalledMeter>[] = [
    { header: 'Meter Serial', field: 'meterSerial', sortable: true },
    { header: 'Status', field: 'status' },
    { header: 'Meter model', field: 'meterModel' },
    { header: 'Meter type', field: 'meterType' },
    { header: 'Meter make', field: 'meterMake' },
    { header: 'Installation Date', field: 'installationDate', sortable: true },
    { header: 'Assigned To', field: 'contractorName' },
    {
      header: 'Actions',
      field: 'action' as keyof InstalledMeter,
      width: '180px',
      pinned: 'right',
      right: '0px',
      type: 'button',
      buttons: (rowData: InstalledMeter) => {
        const baseButtons: MtxGridColumnButton<InstalledMeter>[] = [
          {
            type: 'icon',
            text: 'View',
            icon: 'visibility',
            tooltip: 'View Details',
            click: () => this.openViewDetailsDialog(rowData.meterId, rowData.status),
          },
        ];

        if (rowData.status !== 'OnCustomer') {
          baseButtons.push({
            type: 'icon',
            text: 'Assign to a Contractor',
            icon: 'person_add',
            tooltip: 'Assign to a Contractor',
            click: () => {
              this.openAssignDialog(rowData);
            },
          });
        }

        return baseButtons;
      },
    },
  ];
  filterControls: FilterControl[] = [
    {
      formControlName: 'district',
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
  endpoint: EndPoint = EndPoint.INSTALLED_METERS;
  httpVerb: HttpVerb = HttpVerb.GET;
  contractors: Contractor[] = [];
  private contractorsSubscription!: Subscription;
  private meterDetailsSub!: Subscription;

  private toastr = inject(ToastrService);
  selectedFormat: any;

  constructor(
    private dialog: MatDialog,
    private apiService: ApiService
  ) {}

  ngOnInit(): void {}

  ngOnDestroy(): void {
    if (this.contractorsSubscription) {
      this.contractorsSubscription.unsubscribe();
    }
    if (this.meterDetailsSub) {
      this.meterDetailsSub.unsubscribe();
    }
  }

  private async loadContractors(meterDistrictId: number): Promise<void> {
    try {
      const response = await this.apiService
        .triggerApiRequest<
          BaseResponse<Contractor[]>
        >(EndPoint.ASSIGN_METER_TO_CONTRACTOR, HttpVerb.GET, { districtId: meterDistrictId })
        .toPromise();

      this.contractors = (response as BaseResponse<Contractor[]>).data;
    } catch (error) {
      console.error('Failed to load contractors:', error);
    }
  }

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

  async openAssignDialog(rowData: InstalledMeter): Promise<void> {
    await this.loadContractors(rowData.meterDistrictId);

    const dialogRef = this.dialog.open<AdaptiveDialogComponent>(AdaptiveDialogComponent, {
      width: '400px',
      data: {
        mode: 'form',
        title: 'Assign Contractor',
        fields: [
          {
            label: 'Contractor',
            formControlName: 'contractor',
            type: 'select',
            options: this.contractors,
          },
        ],
      },
    });

    dialogRef.afterClosed().subscribe((result: any) => {
      if (result) {
        this.assignToContractor(rowData, result.contractor);
      }
    });
  }

  openViewDetailsDialog(meterId: number, meterStatus: string): void {
    const url = HelperService.formatEndpoint(EndPoint.INSTALLED_METERS_DETAILS, {
      meterId,
    }) as EndPoint;

    this.apiService
      .triggerApiRequest<BaseResponse<{ oldMeter: BaseMeter; newMeter: BaseMeter }>>(
        url,
        HttpVerb.GET,
        { meterId }
      )
      .pipe(
        switchMap(response => {
          if (!response?.data) {
            throw new Error('Response data is null or undefined.');
          }
          const { oldMeter, newMeter } = response.data;
          return of({ oldMeter, newMeter });
        })
      )
      .subscribe({
        next: ({ oldMeter, newMeter }) => {
          const showTabs = !!oldMeter.meterId;
          this.dialog.open<MeterDetailsDialogComponent>(MeterDetailsDialogComponent, {
            width: '744px',
            data: {
              title: 'Meter Information',
              showTabs,
              oldMeter,
              newMeter,
              meterStatus,
            },
          });
        },
        error: err => {
          console.error('Error loading meter details:', err);
        },
      });
  }

  assignToContractor(meter: InstalledMeter, contractorId: number): void {
    const assignMeterToContractorRequest = { contractorId, meterId: meter.meterId };

    this.apiService
      .triggerApiRequest<
        BaseResponse<boolean>
      >(EndPoint.ASSIGN_METER_TO_AGENT, HttpVerb.POST, null, assignMeterToContractorRequest)
      .subscribe({
        next: () => {
          this.filters = { ...this.filters };
          this.toastr.success('meter assigned to contractor successfully');
        },
        error: err => {
          this.toastr.error('Failed to assign meter to contractor');
        },
      });
  }
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }

  exportMeters(format: string): void {
    if (!format) return;

    const meterSerial = this.filters?.MeterSerial ?? '';
    const districtId = this.filters?.district ?? '';

    const fileType = format;

    this.apiService
      .triggerApiRequest(
        EndPoint.EXPORT_INSTALLED_METERS,
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
