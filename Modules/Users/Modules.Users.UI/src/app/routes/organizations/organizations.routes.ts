import { Routes } from '@angular/router';
import { OrganizationListComponent } from './organization-list/organization-list.component';

export const routes: Routes = [
  {
    path: '',
    component: OrganizationListComponent,
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./create-organization/create-organization.component').then(
        c => c.CreateOrganizationComponent
      ),
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./organization-customers/organization-customers.component').then(
        c => c.OrganizationCustomersComponent
      ),
  },
];
