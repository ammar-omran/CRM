import { CommonModule } from '@angular/common';
import { ChangeDetectorRef, Component, OnInit } from '@angular/core';
import { FormArray, FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatTableModule } from '@angular/material/table';
import {
  InstallMeterRequest,
  MaterialsDetailsDto,
  MeterDto,
  ReplaceMeterRequest,
} from '@shared/interfaces/meter-operations';
import { InstallMeterService } from '@shared/services/install-meter.service';

@Component({
  selector: 'app-meter-operations',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatCardModule,
    MatTableModule,
    ReactiveFormsModule,
  ],
  templateUrl: './meter-operations.component.html',
  styleUrl: './meter-operations.component.scss',
})
export class MeterOperationsComponent implements OnInit {
  installMeterForm: FormGroup;
  meterImageFile: File | null = null;
  displayedColumns: string[] = [
    'materialStatus',
    'materialType',
    'materialQuantity',
    'notes',
    'image',
    'actions',
  ];
  materialDataSource: any[] = []; // DataSource for the table

  get materials(): FormArray {
    return this.installMeterForm.get('materials') as FormArray;
  }

  constructor(
    private fb: FormBuilder,
    private installMeterService: InstallMeterService,
    private cdr: ChangeDetectorRef
  ) {
    this.installMeterForm = this.fb.group({
      customerId: ['', Validators.required],
      customerName: ['', Validators.required],
      meterSerial: ['', Validators.required],
      lastReading: ['' /*Validators.required*/],
      lastPurchase: ['' /*Validators.required*/],
      meterYearOfManufacture: [null],
      meterType: [null, Validators.required],
      meterMake: [null],
      meterModel: [null],
      meterStatus: [null, Validators.required],
      meterImage: [null, Validators.required],
      materials: this.fb.array([]),
    });
  }

  ngOnInit(): void {
    // Initialize the data source from the FormArray
    this.syncDataSource();
  }

  syncDataSource(): void {
    // Update the data source array from the FormArray controls
    this.materialDataSource = this.materials.controls.map(control => control.value);
  }

  // Function to dynamically add material fields
  addMaterial() {
    const materials = this.installMeterForm.get('materials') as FormArray;
    if (materials) {
      materials.push(
        this.fb.group({
          materialStatus: [null, Validators.required],
          materialType: [null],
          materialQuantity: [0, Validators.required],
          notes: [''],
          image: [null],
        })
      );
    }

    this.cdr.detectChanges();

    this.syncDataSource();
  }

  removeMaterial(index: number) {
    const materials = this.installMeterForm.get('materials') as FormArray;
    materials.removeAt(index);

    this.syncDataSource();
  }

  // Handle meter image upload
  onMeterImageChange(event: Event) {
    const fileInput = event.target as HTMLInputElement;
    if (fileInput.files && fileInput.files.length > 0) {
      this.meterImageFile = fileInput.files[0];
      this.installMeterForm.patchValue({ meterImage: this.meterImageFile });
    }
  }

  // Handle material image upload
  onMaterialImageChange(event: Event, index: number) {
    const fileInput = event.target as HTMLInputElement;
    if (fileInput.files && fileInput.files.length > 0) {
      const file = fileInput.files[0];
      this.materials.at(index).patchValue({ image: file });
    }
  }

  // Submit the form
  onSubmit() {
    if (this.installMeterForm.invalid) {
      return;
    }

    const installMeterRequest: InstallMeterRequest = {
      customerId: this.installMeterForm.value.customerId,
      customerName: this.installMeterForm.value.customerName,
      meters: {
        meterSerial: this.installMeterForm.value.meterSerial,
        lastReading: this.installMeterForm.value.lastReading,
        lastPurchase: this.installMeterForm.value.lastPurchase,
        meterYearOfManufacture: this.installMeterForm.value.meterYearOfManufacture,
        meterType: this.installMeterForm.value.meterType,
        meterMake: this.installMeterForm.value.meterMake,
        meterModel: this.installMeterForm.value.meterModel,
        meterStatus: this.installMeterForm.value.meterStatus,
        image: this.installMeterForm.value.meterImage, // Handle file input for image
      },
      materials: this.installMeterForm.value.materials.map((material: MaterialsDetailsDto) => ({
        materialStatus: material.materialStatus,
        materialType: material.materialType,
        materialQuantity: material.materialQuantity,
        notes: material.notes,
        image: material.image, // Handle file input for image
      })),
    };

    /*const replaceMeterRequest: ReplaceMeterRequest = {
      customerId: this.installMeterForm.value.customerId,
      customerName: this.installMeterForm.value.customerName,
      meters: [
        {
          meterSerial: this.installMeterForm.value.meterSerial,
          lastReading: this.installMeterForm.value.lastReading,
          lastPurchase: this.installMeterForm.value.lastPurchase,
          meterYearOfManufacture: this.installMeterForm.value.meterYearOfManufacture,
          meterType: this.installMeterForm.value.meterType,
          meterMake: this.installMeterForm.value.meterMake,
          meterModel: this.installMeterForm.value.meterModel,
          meterStatus: this.installMeterForm.value.meterStatus,
          image: this.installMeterForm.value.meterImage,
        },
      ],

      materials: this.installMeterForm.value.materials.map((material: MaterialsDetailsDto) => ({
        materialStatus: material.materialStatus,
        materialType: material.materialType,
        materialQuantity: material.materialQuantity,
        notes: material.notes,
        image: material.image, // Handle file input for image
      })),
    };*/

    this.installMeterService.submitInstallMeterRequest(installMeterRequest).subscribe(
      response => {
        console.log('Request successful', response);
      },
      error => {
        console.error('Error submitting request', error);
      }
    );
  }
}
