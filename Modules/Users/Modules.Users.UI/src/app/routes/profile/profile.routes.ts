import { Routes } from '@angular/router';
export const routes: Routes = [
  {
    path: 'change-password',
    loadComponent: () =>
      import('./change-password/change-password.component').then(c => c.ChangePasswordComponent),
  },
];
