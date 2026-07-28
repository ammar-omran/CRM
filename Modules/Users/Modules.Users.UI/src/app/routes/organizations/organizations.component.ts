import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslateModule } from '@ngx-translate/core';
import { PageHeaderComponent } from '@shared/components/page-header/page-header.component';
import { OrganizationListComponent } from './organization-list/organization-list.component';
import { OrganizationCustomersComponent } from './organization-customers/organization-customers.component';

@Component({
  selector: 'app-organizations',
  standalone: true,
  imports: [
    CommonModule,
    MatTabsModule,
    TranslateModule,
    PageHeaderComponent,
    OrganizationListComponent,
    OrganizationCustomersComponent,
  ],
  templateUrl: './organizations.component.html',
  styleUrl: './organizations.component.scss',
})
export class OrganizationsComponent {}
