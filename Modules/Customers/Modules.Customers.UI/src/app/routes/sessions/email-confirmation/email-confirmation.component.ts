import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { environment } from '@env/environment';
import { TranslatePipe, TranslateService } from '@ngx-translate/core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';
import { CommonModule } from '@angular/common';
import {
  ElementRef,
  OnDestroy,
  QueryList,
  ViewChildren
} from '@angular/core';
import { BaseResponse } from '@shared/interfaces/base-response';
import { ResponseStatusEnum } from '@shared/Enums/response-status-enum';

@Component({
  selector: 'email-confirmation',
  standalone: true,
  imports: [
    CommonModule,
    MatFormFieldModule,
    MatCardModule,
    ReactiveFormsModule,
    TranslatePipe,
    MatInputModule,
    MatIconModule,
    MatButtonModule,
    RouterModule,
    MatCardModule,
  ],
  templateUrl: './email-confirmation.component.html',
  styleUrl: './email-confirmation.component.scss'
})
export class EmailConfirmationComponent implements OnInit, OnDestroy {

  @ViewChildren('otpInput')
  otpInputs!: QueryList<ElementRef<HTMLInputElement>>;

  emailConfirmationFormGroub!: FormGroup;

  otpControls = new Array(6);

  otpValue: string[] = ['', '', '', '', '', ''];

  disabled = false;
  isExpired = false;
  isLocked = false;
  isSubmitting = false;

  OTPExpiration = environment.ConfirmEmailOTPExpiration;


  coolDown = environment.RetrySendOTP * 60 * 1000;

  attepts = environment.ConfirmEmailOTPAttepts;

  hashedEmail: string | null = null;

  maskedEmail: string | null = null;

  minutes = this.OTPExpiration;

  seconds = 0;

  private timer: any;

  constructor(
    private router: Router,
    private fb: FormBuilder,
    private toastr: ToastrService,
    private apiServices: ApiService,
    private activatedRoute: ActivatedRoute,
    private translateServices: TranslateService
  ) {

    this.activatedRoute.queryParamMap.subscribe(params => {

      this.hashedEmail = params.get('token');

      this.maskedEmail = params.get('maskedEmail');

    });

  }

  ngOnInit(): void {

    this.emailConfirmationFormGroub = this.fb.group({

      otp: [
        '',
        [
          Validators.required,
          Validators.pattern(/^\d{6}$/)
        ]
      ]

    });

    this.startCountdown();

    this.restoreCooldown();

  }

  ngOnDestroy(): void {

    if (this.timer) {
      clearInterval(this.timer);
    }

  }

  private startCountdown(): void {

    let totalSeconds = this.OTPExpiration * 60;

    this.minutes = Math.floor(totalSeconds / 60);

    this.seconds = totalSeconds % 60;

    clearInterval(this.timer);

    this.timer = setInterval(() => {

      totalSeconds--;

      this.minutes = Math.floor(totalSeconds / 60);

      this.seconds = totalSeconds % 60;

      if (totalSeconds <= 0) {

        clearInterval(this.timer);

        this.isExpired = true;

      }

    }, 1000);

  }
  resendMinutes = 0;
  resendSeconds = 0;

  private resendTimer: any;
  private startResendCooldown(duration: number): void {

    clearInterval(this.resendTimer);

    this.disabled = true;

    let totalSeconds = Math.ceil(duration / 1000);

    this.resendMinutes = Math.floor(totalSeconds / 60);
    this.resendSeconds = totalSeconds % 60;

    this.resendTimer = setInterval(() => {

      totalSeconds--;

      this.resendMinutes = Math.floor(totalSeconds / 60);
      this.resendSeconds = totalSeconds % 60;

      if (totalSeconds <= 0) {

        clearInterval(this.resendTimer);

        this.disabled = false;

        localStorage.removeItem('startTime');

      }

    }, 1000);

  }

  private restoreCooldown(): void {

    const startTime = localStorage.getItem('startTime');

    if (!startTime) {
      return;
    }

    const elapsed = Date.now() - Number(startTime);

    const remaining = this.coolDown - elapsed;

    if (remaining <= 0) {
      localStorage.removeItem('startTime');
      return;
    }

    this.startResendCooldown(remaining);
  }

  onOtpInput(event: Event, index: number): void {

    const input = event.target as HTMLInputElement;

    input.value = input.value.replace(/\D/g, '');

    this.otpValue[index] = input.value;

    this.emailConfirmationFormGroub.patchValue({

      otp: this.otpValue.join('')

    });

    if (input.value && index < 5) {

      this.otpInputs.get(index + 1)?.nativeElement.focus();

    }
  }

  onOtpKeyDown(event: KeyboardEvent, index: number): void {

    const input = event.target as HTMLInputElement;

    if (event.key === 'Backspace') {

      if (input.value) {

        input.value = '';

        this.otpValue[index] = '';

      } else if (index > 0) {

        this.otpInputs.get(index - 1)?.nativeElement.focus();

      }

      this.emailConfirmationFormGroub.patchValue({

        otp: this.otpValue.join('')

      });

    }

  }

  onPaste(event: ClipboardEvent): void {

    event.preventDefault();

    const value = event.clipboardData
      ?.getData('text')
      .replace(/\D/g, '')
      .substring(0, 6);

    if (!value) {
      return;
    }

    value.split('').forEach((char, index) => {

      this.otpValue[index] = char;

      const input = this.otpInputs.get(index);

      if (input) {

        input.nativeElement.value = char;

      }

    });

    this.emailConfirmationFormGroub.patchValue({

      otp: value

    });

  }

  private clearOtp(): void {

    this.otpValue = ['', '', '', '', '', ''];

    this.emailConfirmationFormGroub.patchValue({

      otp: ''

    });

    this.otpInputs.forEach(x => x.nativeElement.value = '');

    this.otpInputs.first?.nativeElement.focus();

  }

  resendOtp(): void {
    if (this.disabled || !this.hashedEmail) {
      return;
    }

    localStorage.setItem('startTime', Date.now().toString());
    this.startResendCooldown(this.coolDown);

    this.apiServices
      .triggerApiRequest<BaseResponse<any>>(
        EndPoint.RESEND_OTP,
        HttpVerb.POST,
        null,
        {
          HashedEmail: this.hashedEmail
        }
      )
      .subscribe({
        next: (res) => {
          if (res.status.code == ResponseStatusEnum.Success) {
            this.toastr.success(res.status.message);

            this.isExpired = false;
            this.isLocked = false;

            this.clearOtp();

            this.startCountdown();
          } else {
            this.toastr.error(this.translateServices.instant(res.status.message));
          }
        },
        error: (err) => {
          const errorMsg = err.status?.message ?? this.translateServices.instant('GENERAL_ERROR');
          this.toastr.error(errorMsg);
        }

      });


  }

  otpConfirmation(): void {
    if (
      this.isExpired ||
      this.isLocked ||
      this.emailConfirmationFormGroub.invalid ||
      !this.hashedEmail
    ) {
      return;
    }

    this.isSubmitting = true;

    const formData = {
      HashedEmail: this.hashedEmail,
      Otp: this.emailConfirmationFormGroub.value.otp
    };

    this.apiServices
      .triggerApiRequest<BaseResponse<any>>(
        EndPoint.CONFIRM_EMAIL,
        HttpVerb.POST,
        null,
        formData
      )
      .subscribe({

        next: (res) => {
          if (res.status?.code === ResponseStatusEnum.Success) {
            this.toastr.success(
              this.translateServices.instant(res.status.message)
            );

            this.router.navigate(['/auth/login']);
            return;
          }

          switch (res.status?.message) {
            case 'OTP_WRONG':
              this.clearOtp();

              this.toastr.error(
                this.translateServices.instant('OTP_WRONG')
              );
              break;

            case 'OTP_EXPIRED':
              this.isExpired = true;
              clearInterval(this.timer);
              this.toastr.warning(
                this.translateServices.instant('OTP_EXPIRED')
              );

              break;

            case 'OTP_LOCKED':
              this.isLocked = true;
              clearInterval(this.timer);
              this.toastr.error(
                this.translateServices.instant('OTP_LOCKED')
              );

              break;

            default:
              this.toastr.error(
                this.translateServices.instant('GENERAL_ERROR')
              );
              break;
          }
        },

        error: () => {
          this.toastr.error(
            this.translateServices.instant('GENERAL_ERROR')
          );
        },

        complete: () => {
          this.isSubmitting = false;
        }

      });
  }
}
