import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

export interface BaseResponse<T> {
  data: T;
  status: responseStatus;
  totalItemsCount: number;
}

export interface AcknowledgementResponse {
  status: boolean;
  message: string;
}

export interface responseStatus {
  code: ResponseStatusEnum;
  message: string;
}
