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

  const required: string | string[] | undefined = route.data?.requiredPermission ?? route.data?.permissions;

  // If no permission is declared for the route, allow access
  if (!required) return true;

  // Admin bypasses permission checks
  if (rbac.isAdmin()) return true;

  const requiredList = Array.isArray(required) ? required : [required];
  const hasAny = requiredList.some(p => rbac.hasPermission(p));
  if (hasAny) return true;

  // Unauthorized — redirect to 403
  return router.parseUrl('/403');
};
