import { Component, DestroyRef, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient, HttpParams } from '@angular/common/http';
import { PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MtxGridColumn, MtxGridModule } from '@ng-matero/extensions/grid';
import { TranslateModule } from '@ngx-translate/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject, catchError, debounceTime, of, switchMap } from 'rxjs';
import { environment } from '@env/environment';

import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { FilterComponent } from '@shared/components/filter/filter.component';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import {
  OrganizationCustomerResponse,
  PaginationResponse,
} from '@shared/interfaces/organization.model';

@Component({
  selector: 'app-organization-customers',
  standalone: true,
  imports: [
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MtxGridModule,
    TranslateModule,
    PageHeaderComponent,
    ListActionsComponent,
    FilterComponent,
  ],
  templateUrl: './organization-customers.component.html',
  styleUrl: './organization-customers.component.scss',
})
export class OrganizationCustomersComponent implements OnInit {
  private readonly http = inject(HttpClient);
  private readonly destroyRef = inject(DestroyRef);

  private readonly apiUrl = `${environment.ApiUrl}/Organizations/customers`;

  //Pagination
  pageIndex = 0;
  pageSize = 10;
  totalCount = 0;
  pageSizeOptions = [10, 25, 50, 100];

  //State
  isLoading = false;
  customers: OrganizationCustomerResponse[] = [];
  filterVisible = false;
  isFilterApplied = false;
  activeFilters: Partial<{ name: string; email: string; organizationName: string }> = {};

  private readonly fetchTrigger$ = new BehaviorSubject<void>(undefined);

  //Filter controls
  filterControls: FilterControl[] = [
    {
      formControlName: 'name',
      label: 'Customer Name',
      type: 'text',
    },
    {
      formControlName: 'email',
      label: 'Email',
      type: 'text',
    },
    {
      formControlName: 'organizationName',
      label: 'Organization',
      type: 'text',
    },
  ];

  //Table columns
  columns: MtxGridColumn<OrganizationCustomerResponse>[] = [
    {
      header: 'Customer Name',
      field: 'name',
      sortable: true,
      minWidth: 160,
    },
    {
      header: 'Email',
      field: 'email',
      minWidth: 200,
    },
    {
      header: 'Phone',
      field: 'phone',
      minWidth: 140,
      formatter: (row: OrganizationCustomerResponse) => row.phone ?? '—',
    },
    {
      header: 'Organization',
      field: 'organizationName',
      sortable: true,
      minWidth: 180,
    },
  ];

  ngOnInit(): void {
    this.fetchTrigger$
      .pipe(
        debounceTime(300),
        switchMap(() => {
          this.isLoading = true;
          return this.http
            .get<PaginationResponse<OrganizationCustomerResponse>>(this.apiUrl, {
              params: this.buildParams(),
            })
            .pipe(
              catchError(() => {
                this.customers = [];
                this.totalCount = 0;
                this.isLoading = false;
                return of(null);
              })
            );
        }),
        takeUntilDestroyed(this.destroyRef)
      )
      .subscribe(response => {
        if (response) {
          this.customers = response.items ?? [];
          this.totalCount = response.totalCount ?? 0;
        }
        this.isLoading = false;
      });
  }

  private buildParams(): HttpParams {
    let params = new HttpParams()
      .set('skip', this.pageIndex * this.pageSize)
      .set('limit', this.pageSize);

    if (this.activeFilters.name) {
      params = params.set('name', this.activeFilters.name);
    }
    if (this.activeFilters.email) {
      params = params.set('email', this.activeFilters.email);
    }
    if (this.activeFilters.organizationName) {
      params = params.set('organizationName', this.activeFilters.organizationName);
    }

    return params;
  }

  private resetPagination(): void {
    this.pageIndex = 0;
  }

  toggleFilter(): void {
    this.filterVisible = !this.filterVisible;
  }

  onFilterChanged(filterValues: Record<string, string>): void {
    this.activeFilters = {};

    if (filterValues['name']?.trim()) {
      this.activeFilters.name = filterValues['name'].trim();
    }
    if (filterValues['email']?.trim()) {
      this.activeFilters.email = filterValues['email'].trim();
    }
    if (filterValues['organizationName']?.trim()) {
      this.activeFilters.organizationName = filterValues['organizationName'].trim();
    }

    this.isFilterApplied = Object.keys(this.activeFilters).length > 0;
    this.resetPagination();
    this.fetchTrigger$.next();
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.fetchTrigger$.next();
  }
}
