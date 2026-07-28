import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root',
})
export class HelperService {
  constructor() {}

  static formatEndpoint(endpoint: string, params: { [key: string]: string | number }): string {
    let formattedEndpoint = endpoint;
    for (const key in params) {
      formattedEndpoint = formattedEndpoint.replace(`{${key}}`, params[key].toString());
    }
    return formattedEndpoint;
  }
}
