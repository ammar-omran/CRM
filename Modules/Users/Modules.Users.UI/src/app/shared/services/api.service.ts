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
    options?: { [key: string]: any }
  ): Observable<T> {
    const url = `${environment.ApiUrl}/${endpoint}`;

    const isMockEndpoint = endpoint.includes('mock');
    const httpMethods: Record<HttpVerb, () => Observable<T>> = {
      [HttpVerb.GET]: () =>
        this.httpClient.get<T>(url, isMockEndpoint ? options : { params, ...options }),
      [HttpVerb.POST]: () => this.httpClient.post<T>(url, body, { ...options }),
      [HttpVerb.PUT]: () => this.httpClient.put<T>(url, body, { ...options }),
      [HttpVerb.PATCH]: () => this.httpClient.patch<T>(url, body, { ...options }),
      [HttpVerb.DELETE]: () =>
        this.httpClient.delete<T>(url, isMockEndpoint ? options : { params, ...options }),
    };

    const request = httpMethods[method];
    return request();
  }
}
