import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@env/environment';
import { BaseResponse } from '@shared/interfaces/base-response';
import { RetiredMeterStatusDto } from '@shared/interfaces/dashboard';
import { LookupListDto } from '@shared/interfaces/lookup-list-dto';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class LookupService {
  constructor(private http: HttpClient) {}

  getDistricts(): Observable<BaseResponse<LookupListDto[]>> {
    return this.http.get<BaseResponse<LookupListDto[]>>(`${environment.ApiUrl}/districts/list`);
  }
}
