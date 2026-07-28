// meter.dto.ts
export interface MeterDto {
  meterSerial: string;
  lastReading: string;
  lastPurchase: string;
  meterYearOfManufacture: string | null;
  meterType: number;
  meterMake?: number;
  meterModel?: number;
  image: File | null;
  meterStatus: number;
}

// materials-details.dto.ts
export interface MaterialsDetailsDto {
  materialStatus: number;
  materialType: number | null;
  materialQuantity: number;
  image: File | null;
  notes: string;
}

export interface InstallMeterRequest {
  customerId: string;
  customerName: string;
  meters: MeterDto;
  materials: MaterialsDetailsDto[];
}

export interface ReplaceMeterRequest {
  customerId: string;
  customerName: string;
  meters: MeterDto[];
  materials: MaterialsDetailsDto[];
}
