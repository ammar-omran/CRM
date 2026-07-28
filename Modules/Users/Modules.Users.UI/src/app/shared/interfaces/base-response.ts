import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

export interface BaseResponse<T> {
  data: T;
  status: responseStatus;
  totalItemsCount: number;
}

export interface responseStatus {
  code: ResponseStatusEnum;
  message: string;
}
