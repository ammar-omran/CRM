import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatTabsModule } from '@angular/material/tabs';
import { MeterInfoComponent } from './meter-info/meter-info.component';
import { SparePartsTableComponent } from './spare-parts-table/spare-parts-table.component';
import { BaseMeter, MeterItem } from '@shared/interfaces/meter-info.model';
import { environment } from '@env/environment';
import { DomSanitizer, SafeHtml, SafeUrl } from '@angular/platform-browser';
import { TranslateModule } from '@ngx-translate/core';

interface MeterDetail {
  label: string;
  value: string | number | null;
}
@Component({
  selector: 'app-meter-details-dialog',
  standalone: true,
  imports: [MatTabsModule, MeterInfoComponent, SparePartsTableComponent, TranslateModule],
  templateUrl: './meter-details-dialog.component.html',
  styleUrl: './meter-details-dialog.component.scss',
})
export class MeterDetailsDialogComponent implements OnInit {
  imagePath = this.data.newMeter.image;

  oldMeterDetails: MeterDetail[] = [];
  newMeterDetails: MeterDetail[] = [];
  oldMeterSpareParts: MeterItem[] = [];
  newMeterSpareParts: MeterItem[] = [];
  constructor(
    public dialogRef: MatDialogRef<MeterDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA)
    public data: {
      title: string;
      showTabs: boolean;
      oldMeter: BaseMeter;
      newMeter: BaseMeter;
      meterStatus: string;
    },
    private sanitizer: DomSanitizer
  ) {}

  ngOnInit(): void {
    this.prepareMeterDetails();
  }

  private prepareMeterDetails(): void {
    const formatDate = (dateString: string | null): string =>
      dateString ? new Date(dateString).toLocaleString() : 'N/A';

    if (this.data.oldMeter) {
      this.data.oldMeter.image = this.data.oldMeter.image
        ? `${this.data.oldMeter.image}`
        : 'assets/images/noImage.jpg';
      this.oldMeterDetails = [
        { label: 'Meter Serial', value: this.data.oldMeter.meterSerial },
        { label: 'Final Reading', value: this.data.oldMeter.lastReading },
        { label: 'Meter Type', value: this.data.oldMeter.meterType },
        { label: 'Replacement Reason', value: this.data.oldMeter.replacement_Reason },
        { label: 'Final Balance', value: this.data.oldMeter.lastPurchase },
        { label: 'Meter Display', value: this.data.oldMeter.meterDisplayNotes },
        { label: 'Meter Make', value: this.data.oldMeter.meterMake },
        { label: 'Manufacture Year', value: this.data.oldMeter.meterYearOfManufacture },
      ];
      this.oldMeterSpareParts = this.mapMeterItems(this.data.oldMeter.materialDetails);
      if (this.data.oldMeter) {
        this.oldMeterSpareParts = this.mapMeterItems(this.data.oldMeter.materialDetails) ?? [];
      }
    }

    if (this.data.newMeter) {
      this.data.newMeter.image = this.data.newMeter.image
        ? `${this.data.newMeter.image}`
        : 'assets/images/noImage.jpg';
      this.newMeterDetails = [
        { label: 'Meter Serial', value: this.data.newMeter.meterSerial },
        { label: 'Meter Type', value: this.data.newMeter.meterType },
        { label: 'Installation Type', value: this.data.newMeter.type },
        { label: 'Meter Make', value: this.data.newMeter.meterMake },
        { label: 'Installation Date', value: formatDate(this.data.newMeter.installationDate) },
        { label: 'Meter Model', value: this.data.newMeter.meterModel },
        { label: 'Location', value: this.data.newMeter.location },
      ];
      this.newMeterSpareParts = this.mapMeterItems(this.data.newMeter.materialDetails);
      if (this.data.newMeter) {
        this.newMeterSpareParts = this.mapMeterItems(this.data.newMeter.materialDetails) ?? [];
      }
    }
  }

  private mapMeterItems(materials: MeterItem[]): MeterItem[] {
    return materials.map(({ materialTypeId, materialTypeName, materialQuantity }) => ({
      materialTypeId,
      materialTypeName,
      materialQuantity: materialQuantity || 'N/A',
    }));
  }
  getSanitizedLocation(): SafeHtml {
    if (this.data?.newMeter?.location) {
      return this.sanitizer.bypassSecurityTrustHtml(
        `<a [href]="detail.value" target="_blank">{{ 'view_on_map' | translate }}</a>`
      );
    }
    return '';
  }
}
