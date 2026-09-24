import { inject, Injectable, OnDestroy } from '@angular/core';
import { BehaviorSubject, Observable, Subject, Subscription, timer } from 'rxjs';
import { share } from 'rxjs/operators';

import { LocalStorageService } from '@shared';
import { currentTimestamp, filterObject } from './helpers';
import { Token } from './interface';
import { BaseToken } from './token';
import { TokenFactory } from './token-factory.service';
import { jwtDecode } from 'jwt-decode';
import { JwtPayload } from '@shared/interfaces/jwt-payload.model';
@Injectable({
  providedIn: 'root',
})
export class TokenService implements OnDestroy {
  private key = 'app-token';

  private change$ = new BehaviorSubject<BaseToken | undefined>(undefined);
  private refresh$ = new Subject<BaseToken | undefined>();
  private timer$?: Subscription;

  private _token?: BaseToken;

  private readonly store = inject(LocalStorageService);
  private readonly factory = inject(TokenFactory);

  private get token(): BaseToken | undefined {
    if (!this._token) {
      this._token = this.factory.create(this.store.get(this.key));
    }

    return this._token;
  }

  change(): Observable<BaseToken | undefined> {
    return this.change$.pipe(share());
  }

  refresh(): Observable<BaseToken | undefined> {
    this.buildRefresh();

    return this.refresh$.pipe(share());
  }

  set(token?: Token): TokenService {
    this.save(token);

    return this;
  }

  clear(): void {
    this.save();
  }

  valid(): boolean {
    return this.token?.valid() ?? false;
  }

  getBearerToken(): string {
    return this.token?.getBearerToken() ?? '';
  }

  getAccessToken(): string {
    return this.token?.accessToken ?? '';
  }

  getRefreshToken(): string | void {
    return this.token?.refresh_token;
  }

  ngOnDestroy(): void {
    this.clearRefresh();
  }

  private save(token?: Token): void {
    this._token = undefined;

    if (!token) {
      this.store.remove(this.key);
    } else {
      const value = Object.assign({ accessToken: '', token_type: 'Bearer' }, token, {
        exp: token.expires_in ? currentTimestamp() + token.expires_in : null,
      });
      this.store.set(this.key, filterObject(value));
    }

    this.change$.next(this.token);
    this.buildRefresh();
  }

  private buildRefresh() {
    this.clearRefresh();

    if (this.token?.needRefresh()) {
      this.timer$ = timer(this.token.getRefreshTime() * 1000).subscribe(() => {
        this.refresh$.next(this.token);
      });
    }
  }

  private clearRefresh() {
    if (this.timer$ && !this.timer$.closed) {
      this.timer$.unsubscribe();
    }
  }

  getDecodedToken(): JwtPayload | null {
    const bearer = this.getBearerToken();
    if (!bearer) return null;
    const raw = bearer.startsWith('Bearer ') ? bearer.slice(7) : bearer;
    try {
      return jwtDecode<JwtPayload>(raw);
    } catch {
      return null;
    }
  }

  getUserId(): string | null {
    const p = this.getDecodedToken();
    if (!p) return null;
    return (
      (p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] as string) ??
      p.sub ??
      p.UserId ??
      null
    );
  }

  getUsername(): string | null {
    const p = this.getDecodedToken();
    if (!p) return null;
    return (
      (p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'] as string) ??
      (p as any).name ??
      p.UserName ??
      p.sub ??
      null
    );
  }

  getUserEmail(): string | null {
    const p = this.getDecodedToken();
    if (!p) return null;
    return (
      (p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] as string) ??
      (p as any).email ??
      p.UserEmail ??
      null
    );
  }

  /** Returns the user's primary role name */
  getUserRole(): string | null {
    const p = this.getDecodedToken();
    if (!p) return null;
    const roleClaim =
      (p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] as string | string[]) ??
      (p.role as string | string[]) ??
      p.RoleName;
    if (Array.isArray(roleClaim)) return roleClaim[0] ?? null;
    return (roleClaim as string) ?? null;
  }

  getUserRoles(): string[] {
    const p = this.getDecodedToken();
    if (!p) return [];
    const raw =
      (p['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/role'] as string | string[]) ??
      (p.role as string | string[]) ??
      [];
    if (Array.isArray(raw)) return raw;
    return raw ? [raw as string] : [];
  }

  /**
   * Returns the flat list of permission strings from the JWT.
   * Backend emits `permission` (singular, lowercase) per YARP policy `crm.*`.
   */
  getPermissions(): string[] {
    const p = this.getDecodedToken();
    if (!p) return [];
    const perms =
      (p.permission as string | string[]) ??
      (p.permissions as string | string[]) ??
      (p['permission'] as string | string[]);
    if (Array.isArray(perms)) return perms;
    if (typeof perms === 'string' && perms) return [perms];
    // Fallback: collect any `crm.`-prefixed claim values
    const collected: string[] = [];
    Object.values(p).forEach(v => {
      if (typeof v === 'string' && v.startsWith('crm.')) collected.push(v);
      if (Array.isArray(v)) v.forEach(e => typeof e === 'string' && e.startsWith('crm.') && collected.push(e));
    });
    return collected;
  }
}
