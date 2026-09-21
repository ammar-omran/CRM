import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

export interface BaseResponse<T> {
  data: T;
  status: responseStatus;
  totalItemsCount: number;
  // Backend may also return plain PaginationResponse or DTO directly on 200
  // This keeps backward compat for ApiService that now unwraps both shapes
}

export interface responseStatus {
  code: ResponseStatusEnum;
  message: string;
}

// Backend PaginationResponse — matches CRM.SharedKernel PaginationResponse<T>
export interface PaginationResponse<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalItemsCount: number;
}

// Backend error envelope — ToMVCProblem() -> BaseResponse { Status, Errors }
export interface ApiErrorResponse {
  status: { code: number; message: string };
  errors: Record<string, string>;
}
