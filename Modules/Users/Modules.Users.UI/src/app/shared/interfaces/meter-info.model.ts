export interface BaseMeter {
  meterId: number;
  transactionId: number;
  type: any;
  meterSerial: string;
  lastReading: any;
  lastPurchase: any;
  image: string;
  meterYearOfManufacture: any;
  installationDate: string;
  location: string;
  meterMakeId: number;
  meterModelId: any;
  meterTypeId: number;
  meterType: string;
  replacement_Reason: string;
  meterMake: string;
  meterModel: string;
  meterDisplayNotes: string;
  meterStatus: string;
  materialDetails: MeterItem[];
}

export interface MeterItem {
  materialTypeId: string;
  materialTypeName: string;
  materialQuantity: string;
}
