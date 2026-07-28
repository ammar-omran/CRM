import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { MtxGridColumn } from '@ng-matero/extensions/grid';
import { MatButtonModule } from '@angular/material/button';
import { AdaptiveTableComponent } from '@shared/components/adaptive-table/adaptive-table.component';
import { EndPoint, HttpVerb } from '@shared/enums';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { TranslateModule } from '@ngx-translate/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-organization-list',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    AdaptiveTableComponent,
    PageHeaderComponent,
    TranslateModule
  ],
  templateUrl: './organization-list.component.html',
  styleUrl: './organization-list.component.scss',
})
export class OrganizationListComponent {
  constructor(private router: Router) { }

  hasOrganizations = true;

  endpoint: EndPoint = EndPoint.ORGANIZATIONS_LIST;
  httpVerb: HttpVerb = HttpVerb.GET;

  columns: MtxGridColumn[] = [
    { header: 'Name', field: 'name', sortable: true },
    { header: 'Agents', field: 'agentsCount' },
    { header: 'Customers', field: 'customersCount' },
  ];

  onDataLoaded(items: any[]): void {
    this.hasOrganizations = items && items.length > 0;
  }

  navigateToCreate(): void {
    this.router.navigate(['/organizations/create']);
  }
}

