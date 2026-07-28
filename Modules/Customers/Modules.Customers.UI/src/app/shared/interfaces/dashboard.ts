export interface RetiredMeterStatusDto {
  totalCount: number;
  receivedCount: number;
  notReceivedCount: number;
  stolen: number;
}

export interface MeterStatusDto {
  onCustomer: number;
  onStock: number;
  onAgent: number;
  replaced: number;
  newInstallation: number;
}

export interface ContractorOperationData {
  contractorId: number;
  contractorName: string;
  onAssignCount: number;
  onAgentCount: number;
  onCustomerCount: number;
  newInstallation: number;
  replaced: number;
  totalRetireReceived: number;
  totalRetireNotReceived: number;
  totalCustomerReplacement: number;
  stolen: number;
}
