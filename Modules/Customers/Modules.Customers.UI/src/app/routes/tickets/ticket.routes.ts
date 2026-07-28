import { Routes } from '@angular/router';
import { TicketListComponent } from './ticket-list/ticket-list.component';

export const routes: Routes = [
  {
    path: '',
    component: TicketListComponent,
  },
  {
    path: 'create',
    loadComponent: () =>
      import('./create-ticket/create-ticket.component').then(c => c.CreateTicketComponent),
  },
  {
    path: ':customerId',
    component: TicketListComponent,
  },
  {
    path: ':customerId/create',
    loadComponent: () =>
      import('./create-ticket/create-ticket.component').then(c => c.CreateTicketComponent),
  },
  {
    path: ':ticketId/details',
    loadComponent: () =>
      import('./ticket-details/ticket-details.component').then(c => c.TicketDetailsComponent),
  },
  {
    path: ':ticketId/add-comment',
    loadComponent: () =>
      import('./add-comment/add-comment.component').then(c => c.AddCommentComponent),
  },
];
