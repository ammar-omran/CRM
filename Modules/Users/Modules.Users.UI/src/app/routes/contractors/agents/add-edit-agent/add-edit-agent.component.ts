import { Component, ElementRef, inject, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { Router, ActivatedRoute } from '@angular/router';
import { EndPoint, HttpVerb } from '@shared/enums';
import { Subscription } from 'rxjs';
import { Agent } from '@shared/interfaces/agent.model';
import { BaseResponse } from '@shared/interfaces/base-response';
import { environment } from '@env/environment';
import { StringValidator } from '@shared/validators/is-empty-string';
import { Contractor } from '@shared/interfaces/contractor.model';

@Component({
  selector: 'app-add-edit-agent',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslateModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    MatSelectModule,
    MatCheckboxModule,
  ],
  templateUrl: './add-edit-agent.component.html',
  styleUrl: './add-edit-agent.component.scss',
})
export class AddEditAgentComponent implements OnInit, OnDestroy {
  agentForm!: FormGroup;
  isEditMode = false;
  private apiService = inject(ApiService);
  private toastr = inject(ToastrService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  agentId: string | null = null;
  contractorId: string = '';
  imagePreview: string = 'assets/images/avatar.png';
  private routeSub!: Subscription;
  private agentSub!: Subscription;
  private formSub!: Subscription;
  contractorsSub!: Subscription;
  contractorName?: string;
  @ViewChild('fileInput') fileInput!: ElementRef;

  ngOnInit(): void {
    this.routeSub = this.route.paramMap.subscribe(params => {
      this.agentId = params.get('id');
      this.contractorId = params.get('contractorId') as string;

      this.isEditMode = !!this.agentId;
      this.initializeForm();
      this.fetchContractorDetails();
      if (this.isEditMode) {
        this.loadAgentData(this.agentId!);
      }
    });
  }

  ngOnDestroy(): void {
    if (this.routeSub) {
      this.routeSub.unsubscribe();
    }
    if (this.agentSub) {
      this.agentSub.unsubscribe();
    }
    if (this.formSub) {
      this.formSub.unsubscribe();
    }
    if (this.contractorsSub) {
      this.contractorsSub.unsubscribe();
    }
  }

  private initializeForm(): void {
    this.agentForm = this.fb.group({
      image: [null],
      name: ['', [Validators.required, StringValidator.isEmptyString]],

      ghanaCard: ['', [Validators.required, StringValidator.isEmptyString]],
      mobile: ['', [Validators.required, StringValidator.isEmptyString]],
      email: ['', [Validators.email, Validators.required]],
      status: [true],
    });

    if (this.isEditMode) {
      this.agentForm.addControl(
        'code',
        this.fb.control({ value: '', disabled: true }, Validators.required)
      );
    }
  }

  //TODO: Will be changed based on api response
  private loadAgentData(id: string): void {
    this.agentSub = this.apiService
      .triggerApiRequest<BaseResponse<Agent>>(EndPoint.GET_AGENT_BY_ID, HttpVerb.GET, { id })
      .subscribe({
        next: response => {
          this.agentForm.patchValue({
            name: response.data.name,
            ghanaCard: response.data.ghanaCard,
            mobile: response.data.mobile,
            email: response.data.email,
            status: response.data.isActive,
            code: response.data.code,
          });
          if (response.data.image) {
            this.imagePreview = `data:image/jpeg;base64,${response.data.image}`;
          }

          // this.agentForm.get('name')?.setValue(response.data?.fullName)
        },
      });
  }

  onImageUpload(event: any): void {
    const file = event.target.files[0];
    if (file) {
      if (!file.type.startsWith('image/')) {
        this.toastr.error('Please upload an image file (JPEG, PNG, etc.)');
        this.resetImageSelection();
        return;
      }
      if (file.size > environment.maxImageSize) {
        this.toastr.error(
          `File size should not exceed ${environment.maxImageSize / 1024 / 1024} MB.`
        );
        this.resetImageSelection();
        return;
      }
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
      this.agentForm.get('image')?.setValue(file, { emitEvent: false });
    }
  }

  resetImageSelection(): void {
    this.imagePreview = 'assets/images/avatar.png';
    this.agentForm.get('image')?.setValue(null);
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  fetchContractorDetails(): void {
    this.contractorsSub = this.apiService
      .triggerApiRequest<
        BaseResponse<Contractor>
      >(EndPoint.GET_CONTRACTOR_BY_ID, HttpVerb.GET, { id: this.contractorId })
      .subscribe({
        next: res => {
          this.contractorName = res.data.name;
        },
      });
  }
  onSubmit(): void {
    if (this.agentForm.invalid) return;

    const formValue = this.agentForm.value;
    const formData = new FormData();

    formData.append('contractorId', this.contractorId);
    formData.append('name', formValue.name);
    formData.append('userName', formValue.userName);
    formData.append('ghanaCard', formValue.ghanaCard);
    formData.append('mobile', formValue.mobile);
    formData.append('email', formValue.email);
    formData.append('status', formValue.status);
    formData.append('image', formValue.image);

    if (this.isEditMode) {
      formData.append('id', this.agentId as string);

      this.formSub = this.apiService
        .triggerApiRequest(EndPoint.UPDATE_AGENT, HttpVerb.PUT, null, formData)
        .subscribe({
          next: () => {
            this.toastr.success('Agent updated successfully');
            this.router.navigate(['/contractors']);
          },
        });
    } else {
      this.formSub = this.apiService
        .triggerApiRequest(EndPoint.ADD_AGENT, HttpVerb.POST, null, formData)
        .subscribe({
          next: () => {
            this.toastr.success('Agent added successfully');
            this.router.navigate(['/contractors']);
          },
        });
    }
  }
}
