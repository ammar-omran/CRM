import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from "@angular/material/card";
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { environment } from '@env/environment';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ApiService } from '@shared/services/api.service';
import { ToastrService } from 'ngx-toastr';


@Component({
  selector: 'app-otp-confirmation',
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
  ],
  templateUrl: './otp-confirmation.component.html',
  styleUrl: './otp-confirmation.component.scss'
})
export class OtpConfirmationComponent implements OnInit{
  emailConfirmationFormGroub!: FormGroup;
  disabled: boolean = false;
  OTPExpiration = environment.OTPExpiration;
  coolDown = environment.RetrySendOTP * 60 * 1000;
  hashedEmail: string | null = null;
  lang: string | null = 'en-US';
  ngOnInit(): void {
    this.emailConfirmationFormGroub = this.fb.group({
      otp: ['', [Validators.required]],
    });
    const startTime = localStorage.getItem('startTime');
    if (startTime) {
      const elapsed = Date.now() - parseInt(startTime);
      const remained = this.coolDown - elapsed;
      if (remained > 0) {
        this.disabled = true;
      }
    }
  }
  constructor(
    private router: Router,
     private fb: FormBuilder,
      private toastr: ToastrService,
      private apiServices: ApiService,
      private activatedRoute: ActivatedRoute,
      private translateServices: TranslateService
    ){
    this.activatedRoute.queryParamMap.subscribe(prams => {
      this.hashedEmail = prams.get('token');
      this.lang = prams.get('lang');

    });
    this.translateServices.setDefaultLang('en-US');
    this.translateServices.use(this.lang || 'en-US');

  }
  async optConfirmation(){
  const formData = {
    "hashedEmail" : this.hashedEmail,
    "otp" : `${this.emailConfirmationFormGroub.value.otp}`
  }
  if(this.hashedEmail !== null){
    localStorage.setItem('hashedEmail', this.hashedEmail);
  }
  if(this.lang !== null){
    localStorage.setItem('lang', this.lang);
  }

    await this.apiServices.triggerApiRequest(EndPoint.OPT_CONFIRMATION_WITH_EMAIL, HttpVerb.POST, null, formData).subscribe((ele: any)=>{
      if(ele.status){
        this.toastr.success(
          this.translateServices.instant('otp_sucess')
        )
        this.router.navigate(
          ['/changePassword/reset-password'],
          {queryParams: {token: this.hashedEmail, lang: this.lang}}
        );
      }
      else{
        this.toastr.error(
          this.translateServices.instant('otp_failure')
        )
      }
    })
  }
}
