import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { BaseResponse } from '@shared/interfaces/base-response';
import {
  ContractorOperationData,
  MeterStatusDto,
  RetiredMeterStatusDto,
} from '@shared/interfaces/dashboard';
import { environment } from '@env/environment';

@Injectable()
export class DashboardService {
  constructor(
    private apiService: ApiService,
    private http: HttpClient
  ) { }

  getContractorsOperations(
    districtId: number = 0,
    contractorId: number = 0,
    meterTypeId: number = 0,
    meterMakeId: number = 0,
    startDate?: Date,
    endDate?: Date
  ): Observable<BaseResponse<ContractorOperationData[]>> {
    const params: any = {};

    if (districtId) params.districtId = districtId;
    if (contractorId) params.contractorId = contractorId;
    if (meterTypeId) params.meterTypeId = meterTypeId;
    if (meterMakeId) params.meterMakeId = meterMakeId;
    if (startDate) params.startDate = startDate.toISOString();
    if (endDate) params.endDate = endDate.toISOString();

    return this.http.get<BaseResponse<ContractorOperationData[]>>(
      `${environment.ApiUrl}/Dashboard/operations/statistics`,
      { params }
    );
  }
}
