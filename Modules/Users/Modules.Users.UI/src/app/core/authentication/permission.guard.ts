import { inject } from '@angular/core';
import { ActivatedRouteSnapshot, Router, RouterStateSnapshot } from '@angular/router';
import { RbacService } from './rbac.service';

/**
 * Route guard that enforces permission-based access control.
 *
 * Usage in route definitions:
 * ```ts
 * {
 *   path: 'tickets',
 *   component: TicketListComponent,
 *   canActivate: [permissionGuard],
 *   data: { requiredPermission: 'Tickets.View' }
 * }
 * ```
 *
 * - Admin (no permissions) → redirects to /dashboard
 * - Insufficient permission → redirects to /403
 */
export const permissionGuard = (route: ActivatedRouteSnapshot, _state: RouterStateSnapshot) => {
  const rbac = inject(RbacService);
  const router = inject(Router);

  const required: string | undefined = route.data?.requiredPermission;

  // If no permission is declared for the route, allow access
  if (!required) return true;

  // Admin has no granular permissions — always send to dashboard
  if (rbac.isAdmin()) {
    return router.parseUrl('/dashboard');
  }

  // Check if the user has the required permission
  if (rbac.hasPermission(required)) {
    return true;
  }

  // Unauthorized — redirect to 403
  return router.parseUrl('/403');
};
