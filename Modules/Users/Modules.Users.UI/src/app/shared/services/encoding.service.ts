import { Injectable } from '@angular/core';
import { environment } from '@env/environment';
import * as CryptoJS from 'crypto-js';
import shajs from 'sha.js';

@Injectable({
  providedIn: 'root',
})
export class EncodingService {
  key: string = environment.Key;
  encoding: BufferEncoding = 'hex';
  IV: string = environment.IV;

  constructor() {}
  generateSignature(...values: any[]): string {
    let encryptedData = '';
    for (let i = 0; i < values.length; i++) {
      encryptedData += values[i];
    }
    encryptedData += 'e7db9d7b-fda5-4ede-80ca-486edf679eb7';
    const hashedData = shajs('sha256').update(encryptedData).digest('base64');
    return hashedData;
  }

  encryptPassword(password: string): string {
    const encrypted = CryptoJS.AES.encrypt(password, CryptoJS.enc.Base64.parse(this.key), {
      iv: CryptoJS.enc.Base64.parse(this.IV),
      mode: CryptoJS.mode.CBC,
      padding: CryptoJS.pad.Pkcs7,
    });

    return encrypted.toString();
  }
}
