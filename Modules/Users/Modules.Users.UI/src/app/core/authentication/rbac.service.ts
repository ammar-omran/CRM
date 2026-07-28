import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { TokenService } from './token.service';

export type AppRole = string;

@Injectable({
  providedIn: 'root',
})
export class RbacService {
  private role$ = new BehaviorSubject<AppRole>('');
  private permissions$ = new BehaviorSubject<string[]>([]);

  constructor(private tokenService: TokenService) {}

  /**
   * Call this on startup (and after every token refresh) to decode
   * the JWT and populate the reactive role/permission subjects.
   */
  loadFromToken(): void {
    const role = (this.tokenService.getUserRole() ?? '');
    const permissions = this.tokenService.getPermissions();
    this.role$.next(role);
    this.permissions$.next(permissions);
  }

  /** Clear stored role and permissions (e.g. on logout) */
  clear(): void {
    this.role$.next('');
    this.permissions$.next([]);
  }

  /** Returns the current role as a snapshot string */
  getRole(): AppRole {
    return this.role$.getValue();
  }

  /** Returns the permissions list as a snapshot */
  getPermissionsList(): string[] {
    return this.permissions$.getValue();
  }

  /** Observable of the current role */
  role() {
    return this.role$.asObservable();
  }

  /** Observable of the current permissions list */
  permissions() {
    return this.permissions$.asObservable();
  }

  /** Returns true if the current user has the given permission string */
  hasPermission(permission: string): boolean {
    return this.permissions$.getValue().includes(permission);
  }

  /** Observable that resolves to true/false for a given permission */
  hasPermission$(permission: string) {
    return this.permissions$.pipe(map(perms => perms.includes(permission)));
  }

  isAdmin(): boolean {
    return this.role$.getValue().toLowerCase() === 'admin';
  }

  isSupervisor(): boolean {
    return this.role$.getValue().toLowerCase() === 'supervisor';
  }

  isAgent(): boolean {
    return this.role$.getValue().toLowerCase() === 'agent';
  }
}
