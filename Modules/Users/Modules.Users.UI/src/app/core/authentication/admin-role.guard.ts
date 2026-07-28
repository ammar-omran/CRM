import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { map, take } from 'rxjs/operators';
import { AuthService } from './auth.service';

export const adminRoleGuard = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.user().pipe(
    take(1),
    map(user => {
      const roles: any[] = Array.isArray(user?.roles) ? user.roles : [];
      const hasAdminRole = roles.includes('ROLE-ADM-01');

      return hasAdminRole ? true : router.parseUrl('/403');
    })
  );
};

