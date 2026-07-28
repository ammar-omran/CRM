export type MeterKey = 'onCustomer' | 'onAgent' | 'onStock';

export interface MeterStatResponse {
  data: Record<MeterKey, number>;
  status: {
    code: number;
    message: string;
  };
  totalItemsCount: number;
}
