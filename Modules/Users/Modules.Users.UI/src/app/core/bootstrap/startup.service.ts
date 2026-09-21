import { Injectable } from '@angular/core';
import { AuthService, User } from '@core/authentication';
import { RbacService } from '@core/authentication';
import { NgxPermissionsService, NgxRolesService } from 'ngx-permissions';
import { switchMap, tap } from 'rxjs/operators';
import { Menu, MenuService } from './menu.service';
import { TokenService } from '../authentication/token.service';

@Injectable({
  providedIn: 'root',
})
export class StartupService {
  constructor(
    private authService: AuthService,
    private menuService: MenuService,
    private permissionsService: NgxPermissionsService,
    private rolesService: NgxRolesService,
    private tokenService: TokenService
  ) { }

  /**
   * Load the application only after get the menu or other essential informations
   * such as permissions and roles.
   */
  load() {
    return new Promise<void>((resolve, reject) => {
      this.authService
        .change()
        .pipe(
          tap(user => this.setPermissions(user)),
          switchMap(() => this.authService.menu()),
          tap(menu => this.setMenu(menu))
        )
        .subscribe({
          next: () => resolve(),
          error: () => resolve(),
        });
    });
  }

  private setMenu(menu: Menu[]) {
    this.menuService.addNamespace(menu, 'menu');
    this.menuService.set(menu);
  }

  private setPermissions(user: User) {
    const role = (this.tokenService.getUserRole() ?? '');
    const permissions = this.tokenService.getPermissions();

    this.permissionsService.flushPermissions();

    const rolePermissionToken = role ? `ROLE_${role.toUpperCase()}` : null;
    const allPermissions = rolePermissionToken
      ? [...permissions, rolePermissionToken]
      : permissions;

    this.permissionsService.loadPermissions(allPermissions);

    this.rolesService.flushRoles();
    if (role) {
      this.rolesService.addRoles({ [role.toUpperCase()]: allPermissions });
    }
  }
}
