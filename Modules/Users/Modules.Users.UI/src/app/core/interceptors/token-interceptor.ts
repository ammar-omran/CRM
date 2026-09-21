import { inject, Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
} from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { TokenService } from '@core/authentication';

@Injectable()
export class TokenInterceptor implements HttpInterceptor {
  private readonly tokenService = inject(TokenService);
  private readonly router = inject(Router);

  private readonly excludedUrls = ['/users/login', '/users/register', '/users/refresh', '/users/set-password'];

  intercept(request: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const isExcluded = this.excludedUrls.some(url => request.url.includes(url));
    const shouldAttach = this.tokenService.valid() && !isExcluded;

    const authRequest = shouldAttach
      ? request.clone({
          setHeaders: { Authorization: this.tokenService.getBearerToken() },
        })
      : request;

    return next.handle(authRequest).pipe(
      catchError((error: HttpErrorResponse) => {
        if (error.status === 401 && !isExcluded) {
          this.tokenService.clear();
          this.router.navigateByUrl('/auth/login');
        }
        return throwError(() => error);
      })
    );
  }
}
