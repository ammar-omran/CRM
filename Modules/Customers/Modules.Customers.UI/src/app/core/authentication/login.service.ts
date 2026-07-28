import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, tap } from 'rxjs/operators';

import { admin, Menu } from '@core';
import { Token } from './interface';
import { of } from 'rxjs';
import { environment } from '@env/environment';
import { EncodingService } from '@shared/services/encoding.service';

@Injectable({
  providedIn: 'root',
})
export class LoginService {
  constructor(
    protected http: HttpClient,
    private _encodingService: EncodingService
  ) {}

  login(email: string, password: string) {
    // password = this._encodingService.encryptPassword(password) as string;

    const emailUrl = `${environment.ApiUrl}/customer/login`;
    return this.http
      .post<Token>(emailUrl, {
        email,
        password,
      })
      .pipe(
        // Helpful during 404 debugging
        // eslint-disable-next-line rxjs/no-ignored-error
        // @ts-ignore - tap error callback
        tap({
          error: (err: any) => {
            if (err?.status === 404) {
              // Log full URL so dev can verify base and path
              // eslint-disable-next-line no-console
              console.error('[Login Email] 404 URL:', emailUrl);
            }
          },
        }),
        map((response: any) => {
          // Normalize to a consistent object shape
          const token = response?.token
            || response?.accessToken
            || response?.data?.accessToken
            || response;

          return {
            token,
            customerId: response?.customerId,
            customerName: response?.customerName,
            customerEmail: response?.customerEmail,
          } as {
            token: string;
            customerId?: number;
            customerName?: string;
            customerEmail?: string;
          };
        })
      );
  }

  loginWithPhone(dto: { phoneNumber: string; countryCode: string; password: string }) {
    // password = this._encodingService.encryptPassword(password) as string;

    const digitsOnly = String(dto.phoneNumber).replace(/\D/g, '');
    let countryCode = String(dto.countryCode || '').replace(/\s/g, '');
    // Ensure DB format: country code with leading '+' always
    if (!countryCode.startsWith('+')) {
      countryCode = `+${countryCode}`;
    }

    const payload = {
      phoneNumber: {
        number: digitsOnly,
        countryCode, // e.g. "+20" for Egypt
      },
      password: dto.password,
    };

    const url = `${environment.ApiUrl}/customer/login-with-phone`;
    return this.http
      .post<Token>(url, payload)
      .pipe(
        // Helpful during 404 debugging
        // eslint-disable-next-line rxjs/no-ignored-error
        // @ts-ignore - tap error callback
        tap({
          error: (err: any) => {
            if (err?.status === 404) {
              // eslint-disable-next-line no-console
              console.error('[Login With Phone] 404 URL:', url, 'payload:', payload);
            }
          },
        }),
        map((response: any) => {
          const token = response?.token
            || response?.accessToken
            || response?.data?.accessToken
            || response;

          return {
            token,
            customerId: response?.customerId,
            customerName: response?.customerName,
            customerEmail: response?.customerEmail,
          } as {
            token: string;
            customerId?: number;
            customerName?: string;
            customerEmail?: string;
          };
        })
      );
  }

  refresh(params: Record<string, any>) {
    return this.http.post<Token>('/auth/refresh', params);
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
