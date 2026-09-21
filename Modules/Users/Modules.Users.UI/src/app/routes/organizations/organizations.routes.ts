import { Routes } from '@angular/router';
import { OrganizationListComponent } from './organization-list/organization-list.component';
import { rolesGuard } from '@core/authentication/roles.guard';

export const routes: Routes = [
  {
    path: '',
    component: OrganizationListComponent,
    canActivate: [rolesGuard],
    data: { only: ['admin', 'supervisor'] },
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./create-organization/create-organization.component').then(
        c => c.CreateOrganizationComponent
      ),
    canActivate: [rolesGuard],
    data: { only: 'admin' },
  },
  {
    path: 'customers',
    loadComponent: () =>
      import('./organization-customers/organization-customers.component').then(
        c => c.OrganizationCustomersComponent
      ),
  },
];
