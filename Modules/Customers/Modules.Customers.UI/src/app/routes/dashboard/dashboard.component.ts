import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatGridListModule } from '@angular/material/grid-list';
import { MatListModule } from '@angular/material/list';
import { MatTableModule } from '@angular/material/table';
import { MatTabsModule } from '@angular/material/tabs';
import { MtxProgressModule } from '@ng-matero/extensions/progress';
import { DashboardService } from './dashboard.service';
import { DashboardHeaderComponent } from './dashboard-header/dashboard-header.component';
import { MeterStatsCardComponent } from './meter-stats-card/meter-stats-card.component';
import { DashboardTableComponent } from './dashboard-table/dashboard-table.component';
import { MeterStatusDto, RetiredMeterStatusDto } from '@shared/interfaces/dashboard';
import { BaseResponse } from '@shared/interfaces/base-response';
import { ApiService } from '@shared/services/api.service';
import { BehaviorSubject, Observable, combineLatest, switchMap, map } from 'rxjs';
import { EndPoint, HttpVerb } from '@shared/enums';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  providers: [DashboardService],
  standalone: true,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatChipsModule,
    MatListModule,
    MatGridListModule,
    MatTableModule,
    MatTabsModule,
    MtxProgressModule,
    DashboardHeaderComponent,
    MeterStatsCardComponent,
    DashboardTableComponent,
    AsyncPipe,
  ],
})
export class DashboardComponent implements OnInit {
  private apiService = inject(ApiService);

  isToggleApplied = false;
  selectedDistrict: number = 0;

  private districtSubject = new BehaviorSubject<number>(1);
  public filtersSubject = new BehaviorSubject<{
    contractorId: number;
    meterTypeId: number;
    meterMakeId: number;
    startDate?: Date;
    endDate?: Date;
  }>({
    contractorId: 0,
    meterTypeId: 0,
    meterMakeId: 0,
  });

  meters$!: Observable<{
    stats: { key: keyof MeterStatusDto; label: string; color: string }[];
    total: number;
    values: MeterStatusDto;
    chartId: string;
    chartOptions: any;
  }>;

  retiredMeters$!: Observable<{
    stats: { key: keyof RetiredMeterStatusDto; label: string; color: string }[];
    total: number;
    values: Omit<RetiredMeterStatusDto, 'totalCount'>;
    chartId: string;
    chartOptions: any;
  }>;

  ngOnInit(): void {
    this.fetchData();
  }

  onDashboardFilterChanged(filter: any): void {
    this.selectedDistrict = filter.district || 0;
    this.districtSubject.next(this.selectedDistrict);
    this.filtersSubject.next({
      contractorId: filter.contractor || 0,
      meterMakeId: filter.meterMake || 0,
      meterTypeId: filter.meterType || 0,
      startDate: filter.startDate,
      endDate: filter.endDate,
    });
    this.isToggleApplied = !this.isToggleApplied;
  }

  private fetchData(): void {
    this.meters$ = combineLatest([this.districtSubject, this.filtersSubject]).pipe(
      switchMap(([districtId, filters]) => {
        const params: any = { districtId };

        if (filters.contractorId) params.contractorId = filters.contractorId;
        if (filters.meterMakeId) params.meterMakeId = filters.meterMakeId;
        if (filters.meterTypeId) params.meterTypeId = filters.meterTypeId;
        if (filters.startDate) params.startDate = filters.startDate.toISOString();
        if (filters.endDate) params.endDate = filters.endDate.toISOString();

        return this.apiService
          .triggerApiRequest<
            BaseResponse<MeterStatusDto>
          >(`${EndPoint.METERS_STATISTICS}` as EndPoint, HttpVerb.GET, params)
          .pipe(
            map(response => {
              const statsConfig = [
                { key: 'onCustomer' as const, label: 'On Customer', color: '#A84E4E' },
                { key: 'onAgent' as const, label: 'On Agent', color: '#C7A6A6' },
                { key: 'onStock' as const, label: 'In Stock', color: '#6C1414' },
                { key: 'replaced' as const, label: 'Replacement', color: '#A84E4E' },
                { key: 'newInstallation' as const, label: 'New Installation', color: '#dc143c' },
              ];

              return {
                stats: statsConfig,
                total:
                  (response.data?.onCustomer ?? 0) +
                  (response.data?.onStock ?? 0) +
                  (response.data?.onAgent ?? 0),
                values: response.data ?? {},
                chartId: 'metersChart',
                chartOptions: {
                  chart: {
                    type: 'pie',
                    height: 180,
                    width: 150,
                  },
                  labels: statsConfig.map(s => s.label),
                  series: statsConfig.map(s => response.data?.[s.key] ?? 0),
                  colors: statsConfig.map(s => s.color),
                  legend: { show: false },
                  stroke: { width: 0 },
                },
              };
            })
          );
      })
    );

    this.retiredMeters$ = combineLatest([this.districtSubject, this.filtersSubject]).pipe(
      switchMap(([district, filters]) => {
        const url = `${EndPoint.RETIRED_METER_STATISTICS}`;
        const params: any = {
          DistrictID: district,
          contractorId: filters.contractorId,
          meterMakeId: filters.meterMakeId,
          meterTypeId: filters.meterTypeId,
        };

        if (filters.startDate) params.startDate = filters.startDate.toISOString();
        if (filters.endDate) params.endDate = filters.endDate.toISOString();

        return this.apiService
          .triggerApiRequest<
            BaseResponse<RetiredMeterStatusDto>
          >(url as EndPoint, HttpVerb.GET, params)
          .pipe(
            map(response => {
              const statsConfig = [
                { key: 'receivedCount' as const, label: 'Received', color: '#303f9f' },
                { key: 'notReceivedCount' as const, label: 'Not Received', color: '#7986cb' },
                { key: 'stolen' as const, label: 'Stolen', color: '#c5cae9' },
              ];

              return {
                stats: statsConfig,
                total: response.data.totalCount ?? 0,
                values: {
                  receivedCount: response.data.receivedCount ?? 0,
                  notReceivedCount: response.data.notReceivedCount ?? 0,
                  stolen: response.data.stolen,
                },
                chartId: 'retiredMetersChart',
                chartOptions: {
                  chart: {
                    type: 'pie',
                    height: 180,
                    width: 150,
                  },
                  labels: statsConfig.map(s => s.label),
                  series: statsConfig.map(s => response.data?.[s.key] ?? 0),
                  colors: statsConfig.map(s => s.color),
                  legend: { show: false },
                  stroke: { width: 0 },
                },
              };
            })
          );
      })
    );
  }
}
