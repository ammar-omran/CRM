import { DatePipe } from '@angular/common';
import { Component, EventEmitter, Output } from '@angular/core';
import { FilterComponent } from '../../../shared/components/filter/filter.component';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { EndPoint } from '@shared/enums';
import { ListActionsComponent } from '../../../shared/components/list-actions/list-actions.component';
import { PageHeaderComponent } from '../../../shared/components/page-header/page-header.component';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [DatePipe, FilterComponent, ListActionsComponent, PageHeaderComponent],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.scss',
})
export class DashboardHeaderComponent {
  filterVisible = false;
  toggleFilter() {
    this.filterVisible = !this.filterVisible;
  }
  @Output() filtersChanged = new EventEmitter<any>();
  currentDate = new Date();
  filterControls: FilterControl[] = [
    {
      formControlName: 'district',
      label: 'District',
      type: 'select',
      apiEndpoint: EndPoint.DISTRICTS,
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
      formControlName: 'contractor',
      label: 'Contractor',
      type: 'select',
      apiEndpoint: EndPoint.CONTRACTORS,
    },
    {
      formControlName: 'meterType',
      label: 'Meter Type',
      type: 'select',
      apiEndpoint: EndPoint.METER_TYPES,
    },
    {
      formControlName: 'meterMake',
      label: 'Meter Make',
      type: 'select',
      apiEndpoint: EndPoint.METER_MAKES,
    },
  ];

  onFilterChanged(filterValues: any): void {
    this.filtersChanged.emit(filterValues); // send full filter object
  }
}
