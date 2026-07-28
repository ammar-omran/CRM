import { Routes } from '@angular/router';
import { authGuard } from '@core';
import { guestGuard } from '@core/authentication/auth.guard';
import { AdminLayoutComponent } from '@theme/admin-layout/admin-layout.component';
import { AuthLayoutComponent } from '@theme/auth-layout/auth-layout.component';
import { DashboardComponent } from './routes/dashboard/dashboard.component';
import { Error403Component } from './routes/sessions/403.component';
import { Error404Component } from './routes/sessions/404.component';
import { Error500Component } from './routes/sessions/500.component';
import { LoginComponent } from './routes/sessions/login/login.component';
import { TicketDetailsComponent } from './routes/tickets/ticket-details/ticket-details.component';
import { TicketListComponent } from './routes/tickets/ticket-list/ticket-list.component';
import { CreateTicketComponent } from './routes/tickets/create-ticket/create-ticket.component';
import { RegisterComponent } from './routes/sessions/register/register.component';
import { UpdatePasswordInitComponent } from './routes/profile/change-password/update-password-init/update-password-init.component';
import { UpdatePasswordComponent } from './routes/profile/change-password/update-password/update-password.component';
import { OtpConfirmationComponent } from './routes/profile/change-password/otp-confirmation/otp-confirmation.component';
import { ResetPasswordWithEmailComponent } from './routes/profile/change-password/reset-password-with-email/reset-password-with-email.component';
import { ChangePasswordLayoutComponent } from '@theme/change-password-layout/change-password-layout.component';

export const routes: Routes = [
  {
    path: '',
    component: AdminLayoutComponent,
    canActivate: [authGuard],
    canActivateChild: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      {
        path: 'tickets',
        loadChildren: () => import('./routes/tickets/ticket.routes').then(m => m.routes),
      },

      {
        path: 'contractors',
        loadChildren: () => import('./routes/contractors/contractors.routes').then(m => m.routes),
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
  {
    path: 'auth',
    component: AuthLayoutComponent,
    children: [
      { path: 'login', component: LoginComponent, canActivate: [guestGuard] },
      { path: 'register', component: RegisterComponent },
    ],
  },
  {
    path: 'changePassword',
    component: ChangePasswordLayoutComponent,
    children: [
      { path: 'change-password-init', component: UpdatePasswordInitComponent },
      { path: 'update-password', component: UpdatePasswordComponent },
      { path: 'otp-confirmation', component: OtpConfirmationComponent },
      { path: 'reset-password', component: ResetPasswordWithEmailComponent },
    ],
  },

  { path: '**', redirectTo: 'dashboard' },
];
