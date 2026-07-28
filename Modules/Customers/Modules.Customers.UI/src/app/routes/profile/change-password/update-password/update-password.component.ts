import { Component, OnInit } from '@angular/core';
import { MatCardModule } from '@angular/material/card';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ToastrService } from 'ngx-toastr';
import { Subject, takeUntil, timer } from 'rxjs';
import { CommonModule } from '@angular/common';
import { environment } from '@env/environment';

@Component({
  selector: 'app-update-password',
  standalone: true,
  imports: [
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslateModule,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    RouterModule,
    MatCardModule,
    CommonModule
  ],
  templateUrl: './update-password.component.html',
  styleUrl: './update-password.component.scss',
})
export class UpdatePasswordComponent implements OnInit {
  email: string = '';
  disabled: boolean = false;
  coolDown = environment.RetrySendOTP * 60 * 1000;
  ngOnInit(): void {
    const startTime = localStorage.getItem('startTime');

    if (startTime) {
      const elapsed = Date.now() - parseInt(startTime);
      const remained = this.coolDown - elapsed;
      if (remained > 0) {
        this.disabled = true;
      }


    }
    this.updatePasswordFormGroub = this.fb.group({
      email: ['', [
        Validators.required,
         Validators.email,
         Validators.pattern(/^[a-zA-Z0-9._%+-]+@(gmail\.com|yahoo\.com|outlook\.com)$/)
        ]],
    });


  }
  updatePasswordFormGroub!: FormGroup;
  constructor(
    private fb: FormBuilder,
    private router: Router,
    private apiServices: ApiService,
    private toastr: ToastrService,
    private translateServices: TranslateService
  ) {
    this.translateServices.setDefaultLang('en-US');
    this.translateServices.use(localStorage.getItem('lang') || 'en-US');
  }
  otpConfirmation() {
    this.router.navigate(['/changePassword//otp-confirmation']);
  }
  async updatePassword() {
    localStorage.setItem('startTime', Date.now().toString());
    this.disabled = true;
    const formData = {
      email: this.updatePasswordFormGroub.value.email,
      lang: localStorage.getItem('lang') || 'en-US',
    };
    await this.apiServices
      .triggerApiRequest(EndPoint.UPDATE_PASSWORD_WITH_EMAIL, HttpVerb.POST, null, formData)
      .subscribe((ele: any) => {
        this.toastr.success(this.translateServices.instant('send_email_success'));
        localStorage.setItem('startTime', Date.now().toString());

      });
  }
}
