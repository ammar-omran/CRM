import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { AuthService } from './auth.service';
import { RbacService } from './rbac.service';

export const authGuard = (route?: ActivatedRouteSnapshot, state?: RouterStateSnapshot) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.check() ? true : router.parseUrl('/auth/login');
};

export const guestGuard = (route?: ActivatedRouteSnapshot, state?: RouterStateSnapshot) => {
  const auth = inject(AuthService);
  const rbac = inject(RbacService);
  const router = inject(Router);

  if (!auth.check()) return true;

  // Admin has no ticket permissions — land on dashboard
  // Supervisor & Agent land on ticket list
  return rbac.isAdmin()
    ? router.parseUrl('/dashboard')
    : router.parseUrl('/tickets/0');
};
