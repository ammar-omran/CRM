import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '@env/environment';
import { EndPoint, HttpVerb } from '@shared/enums';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly http = inject(HttpClient);

  /**
   * Typed gateway request — mirrors backend `BaseResponse` envelope:
   * success → direct DTO (e.g. LoginUserResponse, PaginationResponse),
   * error   → BaseResponse { Status, Errors } via ToMVCProblem().
   * Endpoints are already YARP-routed via environment.ApiUrl (gateway /api).
   */
  triggerApiRequest<T>(
    endpoint: EndPoint,
    method: HttpVerb,
    params?: Record<string, string | number | boolean | undefined | null> | null,
    body?: unknown,
    options?: Record<string, unknown>
  ): Observable<T> {
    const isMock = endpoint.includes('mock');
    const url = isMock ? endpoint : `${environment.ApiUrl}/${endpoint}`;

    const httpParams = params ? this.toHttpParams(params) : undefined;
    const baseOptions = { ...(options ?? {}) } as Record<string, unknown>;

    // For GET/DELETE Angular expects { params: HttpParams }
    const getOptions = httpParams ? { params: httpParams, ...baseOptions } : baseOptions;

    switch (method) {
      case HttpVerb.GET:
        return this.http.get<T>(url, getOptions as object);
      case HttpVerb.POST:
        return this.http.post<T>(url, body, baseOptions as object);
      case HttpVerb.PUT:
        return this.http.put<T>(url, body, baseOptions as object);
      case HttpVerb.PATCH:
        return this.http.patch<T>(url, body, baseOptions as object);
      case HttpVerb.DELETE:
        return this.http.delete<T>(url, getOptions as object);
      default:
        throw new Error(`Unsupported HttpVerb: ${method}`);
    }
  }

  private toHttpParams(params: Record<string, unknown>): HttpParams {
    let httpParams = new HttpParams();
    Object.entries(params).forEach(([key, value]) => {
      if (value === undefined || value === null || value === '') return;
      if (Array.isArray(value)) {
        value.forEach(v => {
          if (v !== undefined && v !== null) httpParams = httpParams.append(key, String(v));
        });
      } else {
        httpParams = httpParams.set(key, String(value));
      }
    });
    return httpParams;
  }
}
