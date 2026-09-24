import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { catchError, map } from 'rxjs/operators';
import { Observable, throwError, of } from 'rxjs';

import { admin, Menu } from '@core';
import { Token } from './interface';
import { environment } from '@env/environment';

export interface LoginResponse {
  token: string;
  refreshToken: string;
}

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  private readonly http = inject(HttpClient);

  login(email: string, password: string): Observable<LoginResponse> {
    // Backend expects { Email, Password } at POST api/customers/login
    return this.http
      .post<LoginResponse>(`${environment.ApiUrl}/customers/login`, { email, password })
      .pipe(
        map(res => ({
          token: (res as any).token ?? (res as any).Token,
          refreshToken: (res as any).refreshToken ?? (res as any).RefreshToken,
        })),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
        })
      );
  }

  loginWithPhone(dto: { phoneNumber: string; countryCode: string; password: string }): Observable<LoginResponse> {
    const digitsOnly = String(dto.phoneNumber).replace(/\D/g, '');
    let countryCode = String(dto.countryCode || '').replace(/\s/g, '');
    if (!countryCode.startsWith('+')) {
      countryCode = `+${countryCode}`;
    }
    const payload = {
      phoneNumber: {
        number: digitsOnly,
        countryCode,
      },
      password: dto.password,
    };
    return this.http
      .post<LoginResponse>(`${environment.ApiUrl}/customers/login-with-phone`, payload)
      .pipe(
        map(res => ({
          token: (res as any).token ?? (res as any).Token,
          refreshToken: (res as any).refreshToken ?? (res as any).RefreshToken,
        })),
        catchError(error => {
          console.error('Login with phone error:', error);
          return throwError(() => error);
        })
      );
  }

  refresh(params: { Token: string; RefreshToken: string }) {
    // Backend POST api/customers/refresh expects { Token, RefreshToken }
    return this.http.post<LoginResponse>(`${environment.ApiUrl}/customers/refresh`, params);
  }

  logout() {
    //TODO
    // gonna be fixed when api is ready
    // return this.http.post<any>('/auth/logout', {});
    return of(true);
  }

  me() {
    return of(admin);
  }

  menu() {
    return this.http
      .get<{ menu: Menu[] }>('assets/data/menu.json?_t=' + Date.now())
      .pipe(map(res => res.menu));
  }
}
