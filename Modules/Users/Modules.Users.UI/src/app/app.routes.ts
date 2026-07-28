import { Routes } from '@angular/router';
import { authGuard, guestGuard } from '@core';
import { adminRoleGuard } from './core/authentication/admin-role.guard';
import { AdminLayoutComponent } from '@theme/admin-layout/admin-layout.component';
import { AuthLayoutComponent } from '@theme/auth-layout/auth-layout.component';
import { DashboardComponent } from './routes/dashboard/dashboard.component';
import { Error403Component } from './routes/sessions/403.component';
import { Error404Component } from './routes/sessions/404.component';
import { Error500Component } from './routes/sessions/500.component';
import { LoginComponent } from './routes/sessions/login/login.component';
import { RegisterComponent } from './routes/sessions/register/register.component';

export const routes: Routes = [
  // Default route redirects to auth/login
  { path: '', redirectTo: 'auth/login', pathMatch: 'full' },

  // Auth routes (login, register, etc.)
  {
    path: 'auth',
    component: AuthLayoutComponent,
    canActivate: [guestGuard],
    children: [
      { path: '', redirectTo: 'login', pathMatch: 'full' },
      { path: 'login', component: LoginComponent },
      { path: 'register', component: RegisterComponent },
    ],
  },

  // Protected routes (require authentication)
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: 'dashboard', component: DashboardComponent, pathMatch: 'full' },
      {
        path: 'contractors',
        loadChildren: () => import('./routes/contractors/contractors.routes').then(m => m.routes),
      },
      {
        path: 'tickets',
        loadChildren: () => import('./routes/tickets/tickets.routes').then(m => m.routes),
      },
      {
        path: 'organizations',
        // canActivate: [adminRoleGuard],
        loadChildren: () =>
          import('./routes/organizations/organizations.routes').then(m => m.routes),
      },
      {
        path: 'profile',
        loadChildren: () => import('./routes/profile/profile.routes').then(c => c.routes),
      },
      {
        path: 'retired-meters',
        loadComponent: () =>
          import('./routes/retired-meters/retired-meters.component').then(
            c => c.RetiredMetersComponent
          ),
      },
      {
        path: 'installation',
        loadComponent: () =>
          import('./routes/installations/installations.component').then(
            c => c.InstallationsComponent
          ),
      },
      {
        path: 'audits',
        loadComponent: () =>
          import('./routes/audits/audits.component').then(c => c.AuditsComponent),
      },
      {
        path: 'meter-operations',
        loadComponent: () =>
          import('./routes/meter-operations/meter-operations.component').then(
            c => c.MeterOperationsComponent
          ),
      },
      { path: '403', component: Error403Component },
      { path: '404', component: Error404Component },
      { path: '500', component: Error500Component },
    ],
  },
  // Catch-all route for unknown paths
  { path: '**', redirectTo: 'auth/login' },
];
