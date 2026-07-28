import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@env/environment';
import { EndPoint, HttpVerb } from '@shared/enums';
import { Observable } from 'rxjs';
@Injectable({
  providedIn: 'root',
})
export class ApiService {
  constructor(private httpClient: HttpClient) {}

  triggerApiRequest<T>(
    endpoint: EndPoint,
    method: HttpVerb,
    params?: any,
    body?: any,
    options: { [key: string]: any } = {}
  ): Observable<T> {
    // Remove any leading slashes from the endpoint to prevent double slashes
    const cleanEndpoint = endpoint.replace(/^\/+/, '');
    const url = `${environment.ApiUrl}${environment.ApiUrl.endsWith('/') ? '' : '/'}${cleanEndpoint}`;

    // Determine if this is a multipart/form-data request
    const isFormData = typeof FormData !== 'undefined' && body instanceof FormData;

    // Set default headers; do NOT set Content-Type for FormData (browser will set boundaries)
    const defaultHeaders: Record<string, string> = {
      'Accept': 'application/json',
      ...(isFormData ? {} : { 'Content-Type': 'application/json' }),
    };

    // Merge provided headers with defaults
    const headers = {
      ...defaultHeaders,
      ...(options['headers'] || {})
    };

    const requestOptions = {
      ...options,
      headers: headers
    };

    const isMockEndpoint = endpoint.includes('mock');
    const httpMethods: Record<HttpVerb, () => Observable<T>> = {
      [HttpVerb.GET]: () =>
        this.httpClient.get<T>(url, isMockEndpoint ? requestOptions : { params, ...requestOptions }),
      [HttpVerb.POST]: () => this.httpClient.post<T>(url, body, requestOptions),
      [HttpVerb.PUT]: () => this.httpClient.put<T>(url, body, requestOptions),
      [HttpVerb.PATCH]: () => this.httpClient.patch<T>(url, body, requestOptions),
      [HttpVerb.DELETE]: () =>
        this.httpClient.delete<T>(url, isMockEndpoint ? requestOptions : { params, ...requestOptions }),
    };

    const request = httpMethods[method];
    return request();
  }
}
