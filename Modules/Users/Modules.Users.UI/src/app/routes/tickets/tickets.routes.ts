import { Routes } from '@angular/router';
import { permissionGuard } from '@core';
import { TicketListComponent } from './ticket-list/ticket-list.component';

export const routes: Routes = [
  {
    path: '',
    component: TicketListComponent,
    canActivate: [permissionGuard],
    data: { requiredPermission: '1.2' },
  },
  {
    path: ':ticketId/details',
    loadComponent: () =>
      import('./ticket-details/ticket-details.component').then(c => c.TicketDetailsComponent),
    canActivate: [permissionGuard],
    data: { requiredPermission: '2.2' },
  },
  {
    path: ':ticketId/add-comment',
    loadComponent: () =>
      import('./add-comment/add-comment.component').then(c => c.AddCommentComponent),
    canActivate: [permissionGuard],
    data: { requiredPermission: '4.1' },
  },
  {
    path: '**',
    redirectTo: '0',
  },
];
