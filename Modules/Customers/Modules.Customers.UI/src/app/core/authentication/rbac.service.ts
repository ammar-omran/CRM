import { inject, Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { TokenService } from './token.service';

export type AppRole = string;

@Injectable({
  providedIn: 'root',
})
export class RbacService {
  private readonly tokenService = inject(TokenService);
  private role$ = new BehaviorSubject<AppRole>('');
  private permissions$ = new BehaviorSubject<string[]>([]);
  private roles$ = new BehaviorSubject<string[]>([]);

  constructor() {
    this.loadFromToken();
  }

  /**
   * Call this on startup (and after every token refresh) to decode
   * the JWT and populate the reactive role/permission subjects.
   */
  loadFromToken(): void {
    const role = this.tokenService.getUserRole() ?? '';
    const roles = this.tokenService.getUserRoles();
    const permissions = this.tokenService.getPermissions();
    this.role$.next(role);
    this.roles$.next(roles);
    this.permissions$.next(permissions);
  }

  /** Clear stored role and permissions (e.g. on logout) */
  clear(): void {
    this.role$.next('');
    this.roles$.next([]);
    this.permissions$.next([]);
  }

  getRoles(): string[] {
    return this.roles$.getValue();
  }

  roles() {
    return this.roles$.asObservable();
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
    const all = [this.role$.getValue(), ...this.roles$.getValue()].map(r => r.toLowerCase());
    return all.includes('admin');
  }

  isSupervisor(): boolean {
    const all = [this.role$.getValue(), ...this.roles$.getValue()].map(r => r.toLowerCase());
    return all.includes('supervisor');
  }

  isAgent(): boolean {
    const all = [this.role$.getValue(), ...this.roles$.getValue()].map(r => r.toLowerCase());
    return all.includes('agent');
  }
}
