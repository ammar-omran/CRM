import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@env/environment';
import { InstallMeterRequest, ReplaceMeterRequest } from '@shared/interfaces/meter-operations';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class InstallMeterService {
  private apiUrl = `${environment.ApiUrl}/Meters/install`; // Replace with your actual endpoint
  // private apiUrl = `${environment.ApiUrl}/Meters/replace`; // Replace with your actual endpoint

  constructor(private http: HttpClient) {}

  submitInstallMeterRequest(request: InstallMeterRequest): Observable<any> {
    const formData = new FormData();

    // Append Meter Details
    formData.append('IMEI', 'be1fb63d587797e5');
    formData.append(
      'Signature',
      '9E9B2130BF6634E1B31AC9D63946F2C01F386FFB182989F351CC7AE29C504391487A6521A7316CD364A56B931773E742D3C61F5AFA0C42F28D8A5747B8D7B36A'
    );
    formData.append('customerId', request.customerId);
    formData.append('customerName', request.customerName);

    const meter = request.meters;
    formData.append('meters.meterSerial', meter.meterSerial);
    if (meter.lastReading) formData.append('meters.lastReading', meter.lastReading);
    if (meter.lastPurchase) formData.append('meters.lastPurchase', meter.lastPurchase);
    formData.append('meters.meterYearOfManufacture', meter.meterYearOfManufacture || '');
    formData.append('meters.meterType', meter.meterType.toString());
    if (meter.image) formData.append('meters.image', meter.image);
    if (meter.meterMake) formData.append('meters.meterMake', meter.meterMake.toString());
    if (meter.meterModel) formData.append('meters.meterModel', meter.meterModel.toString());
    if (meter.image) formData.append('meters.image', meter.image);
    formData.append('meters.meterStatus', meter.meterStatus.toString());

    // Append Material Details
    request.materials.forEach((material, index) => {
      formData.append(`materials[${index}].materialStatus`, material.materialStatus.toString());
      formData.append(`materials[${index}].materialType`, material.materialType?.toString() || '');
      formData.append(`materials[${index}].materialQuantity`, material.materialQuantity.toString());
      formData.append(`materials[${index}].notes`, material.notes);
      if (material.image) formData.append(`materials[${index}].image`, material.image);
    });

    return this.http.post(this.apiUrl, formData, {
      headers: new HttpHeaders(),
    });
  }

  submitReplaceMeterRequest(request: ReplaceMeterRequest): Observable<any> {
    const formData = new FormData();

    // Append Meter Details
    formData.append('IMEI', 'be1fb63d587797e5');
    formData.append(
      'Signature',
      '9E9B2130BF6634E1B31AC9D63946F2C01F386FFB182989F351CC7AE29C504391487A6521A7316CD364A56B931773E742D3C61F5AFA0C42F28D8A5747B8D7B36A'
    );
    formData.append('customerId', request.customerId);
    formData.append('customerName', request.customerName);

    request.meters.forEach((meter, index) => {
      formData.append(`meters[${index}].meterSerial`, meter.meterSerial);
      if (meter.lastReading) formData.append(`meters[${index}].lastReading`, meter.lastReading);
      if (meter.lastPurchase) formData.append(`meters[${index}].lastPurchase`, meter.lastPurchase);
      formData.append(
        `meters[${index}].meterYearOfManufacture`,
        meter.meterYearOfManufacture || ''
      );
      formData.append(`meters[${index}].meterType`, meter.meterType.toString());
      if (meter.image) formData.append(`meters[${index}].image`, meter.image);
      if (meter.meterMake)
        formData.append(`meters[${index}].meterMake`, meter.meterMake.toString());
      if (meter.meterModel)
        formData.append(`meters[${index}].meterModel`, meter.meterModel.toString());
      if (meter.image) formData.append(`meters[${index}].image`, meter.image);
      formData.append(`meters[${index}].meterStatus`, meter.meterStatus.toString());
    });
    const meter = request.meters;

    // Append Material Details
    request.materials.forEach((material, index) => {
      formData.append(`materials[${index}].materialStatus`, material.materialStatus.toString());
      formData.append(`materials[${index}].materialType`, material.materialType?.toString() || '');
      formData.append(`materials[${index}].materialQuantity`, material.materialQuantity.toString());
      formData.append(`materials[${index}].notes`, material.notes);
      if (material.image) formData.append(`materials[${index}].image`, material.image);
    });

    return this.http.post(this.apiUrl, formData, {
      headers: new HttpHeaders(),
    });
  }
}
