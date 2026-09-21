import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { RbacService } from './rbac.service';

export const rolesGuard = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
  const rbac = inject(RbacService);
  const router = inject(Router);

  const only: string[] | undefined = route.data?.only;

  if (!only) return true;

  if (only.includes(rbac.getRole())) {
    return true;
  }

  // Unauthorized — redirect to 403
  return router.parseUrl('/dashboard');
};
