import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, iif, merge, of } from 'rxjs';
import { catchError, map, share, switchMap, tap } from 'rxjs/operators';
import { isEmptyObject } from './helpers';
import { Token, User } from './interface';
import { LoginService } from './login.service';
import { TokenService } from './token.service';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly loginService = inject(LoginService);
  private readonly tokenService = inject(TokenService);
  private user$ = new BehaviorSubject<User>({});
  private change$ = merge(
    this.tokenService.change(),
    this.tokenService.refresh().pipe(switchMap(() => this.refresh()))
  ).pipe(
    switchMap(() => this.assignUser()),
    share()
  );

  init() {
    return new Promise<void>(resolve => this.change$.subscribe(() => resolve()));
  }

  change() {
    return this.change$;
  }

  check() {
    return this.tokenService.valid();
  }

  login(email: string, password: string) {
    return this.loginService.login(email, password).pipe(
      tap(res => {
        const token: Token = {
          accessToken: res.token,
          refresh_token: res.refreshToken,
        };
        this.tokenService.set(token);
        // Persist customer info if backend returns it alongside token
        const { customerId, customerName, customerEmail } = (res as any) || {};
        if (customerId) {
          try { localStorage.setItem('customerId', String(customerId)); } catch {}
        }
        if (customerName) {
          try { localStorage.setItem('customerName', String(customerName)); } catch {}
        }
        if (customerEmail) {
          try { localStorage.setItem('customerEmail', String(customerEmail)); } catch {}
        }
      }),
      map(() => this.check())
    );
  }

  loginWithPhone(dto: { phoneNumber: string; countryCode: string; password: string }) {
    return this.loginService.loginWithPhone(dto).pipe(
      tap(res => {
        const token: Token = {
          accessToken: res.token,
          refresh_token: res.refreshToken,
        };
        this.tokenService.set(token);
        const { customerId, customerName, customerEmail } = (res as any) || {};
        if (customerId) {
          try { localStorage.setItem('customerId', String(customerId)); } catch {}
        }
        if (customerName) {
          try { localStorage.setItem('customerName', String(customerName)); } catch {}
        }
        if (customerEmail) {
          try { localStorage.setItem('customerEmail', String(customerEmail)); } catch {}
        }
      }),
      map(() => this.check())
    );
  }

  refresh() {
    const refreshToken = this.tokenService.getRefreshToken() as string | undefined;
    const accessToken = this.tokenService.getAccessToken();
    if (!refreshToken || !accessToken) return of(false);
    return this.loginService
      .refresh({ Token: accessToken, RefreshToken: refreshToken })
      .pipe(
        catchError(() => of(undefined as unknown as Token)),
        tap(token => {
          if (token) {
            const t = token as unknown as { token: string; refreshToken: string; Token: string; RefreshToken: string };
            this.tokenService.set({
              accessToken: t.token ?? t.Token,
              refresh_token: t.refreshToken ?? t.RefreshToken,
            } as Token);
          }
        }),
        map(() => this.check())
      );
  }

  logout() {
    return this.loginService.logout().pipe(
      tap(() => {
        this.tokenService.clear();
        try {
          localStorage.removeItem('customerId');
          localStorage.removeItem('customerName');
          localStorage.removeItem('customerEmail');
        } catch {}
      }),
      map(() => !this.check())
    );
  }

  user() {
    return this.user$.pipe(share());
  }

  menu() {
    return iif(() => this.check(), this.loginService.menu(), of([]));
  }

  private assignUser() {
    if (!this.check()) {
      return of({}).pipe(tap(user => this.user$.next(user)));
    }

    if (!isEmptyObject(this.user$.getValue())) {
      return of(this.user$.getValue());
    }

    return this.loginService.me().pipe(tap(user => this.user$.next(user)));
  }
}
