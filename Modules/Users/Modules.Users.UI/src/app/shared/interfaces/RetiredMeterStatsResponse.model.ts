export type RetiredMeterKey = 'receivedCount' | 'notReceivedCount' | 'totalCount';

export interface RetiredMeterStatResponse {
  data: Record<RetiredMeterKey, number>;
  status: {
    code: number;
    message: string;
  };
  totalItemsCount: number;
}
