import {
  AfterViewInit,
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  ViewChild,
} from '@angular/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { FilterComponent } from '../../../shared/components/filter/filter.component';
import { EndPoint } from '@shared/enums';
import { DashboardService } from '../dashboard.service';
import { CommonModule } from '@angular/common';
import { ContractorOperationData } from '@shared/interfaces/dashboard';

@Component({
  selector: 'app-dashboard-table',
  standalone: true,
  imports: [CommonModule, MatTableModule, FilterComponent, MatPaginatorModule],
  templateUrl: './dashboard-table.component.html',
  styleUrl: './dashboard-table.component.scss',
})
export class DashboardTableComponent implements OnInit, OnChanges, AfterViewInit {
  dataSource = new MatTableDataSource<ContractorOperationData>();
  @Input() isToggleApplied!: boolean;
  displayedColumns: string[] = [
    'contractorName',
    'onAssignCount',
    'onAgentCount',
    'newInstallation',
    'replaced',
    'totalRetireReceived',
    'totalRetireNotReceived',
    'stolen',
  ];

  filterControls: FilterControl[] = [
    {
      formControlName: 'agent',
      label: 'Agent',
      type: 'select',
      apiEndpoint: EndPoint.AGENTS_LIST,
      optionLabel: 'name',
      optionVal: 'id',
    },
  ];

  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;

  @Input() fullFilters!: {
    district: number;
    contractorId: number;
    meterMakeId: number;
    meterTypeId: number;
    startDate?: Date;
    endDate?: Date;
  };

  constructor(private dashboardService: DashboardService) {}
  ngOnChanges(changes: SimpleChanges): void {
    if (changes.fullFilters || changes.isToggleApplied) {
      this.applyFilter(this.fullFilters);
    }
  }

  ngOnInit(): void {
    this.dashboardService.getContractorsOperations().subscribe(
      response => {
        this.dataSource.data = response.data;
      },
      err => {}
    );
  }

  ngAfterViewInit(): void {
    this.dataSource.paginator = this.paginator;
    this.dataSource.sort = this.sort;
  }

  applyFilter(filters: any): void {
    this.dashboardService
      .getContractorsOperations(
        filters.district,
        filters.contractorId,
        filters.meterTypeId,
        filters.meterMakeId,
        filters.startDate,
        filters.endDate
      )
      .subscribe(response => {
        this.dataSource.data = response.data;
      });
  }
}
