import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, catchError, tap } from 'rxjs/operators';
import { throwError } from 'rxjs';

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
  ) { }

  login(userName: string, password: string) {
    const requestBody = {
      Username: userName,
      password,
    };

    return this.http
      .post<string>(`${environment.ApiUrl}/user/login`, requestBody, {
        responseType: 'text' as 'json',
      })
      .pipe(
        tap((response: string) => { }),
        map((response: string) => {
          try {
            const parsed = JSON.parse(response);
            if (parsed && parsed.token) {
              return parsed.token;
            }
          } catch (e) {
            // response is likely a raw text token rather than JSON
          }
          return response;
        }),
        catchError(error => {
          console.error('Login error:', error);
          return throwError(() => error);
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
