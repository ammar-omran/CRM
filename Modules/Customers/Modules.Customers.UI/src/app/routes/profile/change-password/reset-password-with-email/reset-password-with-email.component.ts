import { Component, OnInit } from '@angular/core';
import { MatCardModule } from "@angular/material/card";
import { MaterialModule } from "../../../../../../schematics/ng-add/files/module-files/app/material.module";
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ApiService } from '@shared/services/api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { ToastrService } from 'ngx-toastr';
import { CustomValidators } from '@shared/utils/custom-validators';
import { environment } from '@env/environment';

@Component({
  selector: 'app-reset-password-with-email',
  standalone: true,
  imports: [
    MatCardModule,
    MaterialModule,
    TranslateModule,
    ReactiveFormsModule
],
  templateUrl: './reset-password-with-email.component.html',
  styleUrl: './reset-password-with-email.component.scss'
})
export class ResetPasswordWithEmailComponent implements OnInit {
  disabled: boolean = false;
  coolDown = environment.RetrySendOTP * 60 * 1000;
  hidePassword: boolean = true;
  hideConfirmPassword: boolean = true;
  resetPasswordFormGroup!: FormGroup;
  ngOnInit(): void {
    const startTime = localStorage.getItem('startTime');

    if (startTime) {
      const elapsed = Date.now() - parseInt(startTime);
      const remained = this.coolDown - elapsed;
      if (remained > 0) {
        this.disabled = true;
      }
    }
    this.resetPasswordFormGroup = this.fb.group({
      password: ['',[Validators.required, CustomValidators.strongPassword()]],
      confirmPassword: ['', [Validators.required]]
    }
  )

  }
  constructor(
    private fb: FormBuilder,
    private apiServices: ApiService,
    private toastr: ToastrService,
    private translateServices: TranslateService
  ){
    this.translateServices.setDefaultLang('en-US');
    this.translateServices.use(localStorage.getItem('lang') || 'en-US');
  }
  pass: String = ''
  conPass: String ='';
  confirmPass(){
    this.pass = this.resetPasswordFormGroup.controls.password.value;
    this.conPass =  this.resetPasswordFormGroup.controls.confirmPassword.value
    if(this.pass !== this.conPass)
      return false;
    else
      return true
  }
  reset_password(){
    localStorage.setItem('startTime', Date.now.toString());
    this.disabled = true;
    if(!this.confirmPass())
      this.toastr.error(
        this.translateServices.instant('not_match_confirm_password')
      )
    else{

    const userData = {
      "hashedEmail": localStorage.getItem('hashedEmail'),
      "password": this.resetPasswordFormGroup.controls.password.value,
      "confirmPassword": this.resetPasswordFormGroup.controls.confirmPassword.value
    }
    this.apiServices
    .triggerApiRequest(
      EndPoint.RESET_PASSWORD_WITH_EMAIL,
       HttpVerb.POST,
        null,
         userData)
    .subscribe((ele: any) => {
      if(ele.status){
        this.toastr.success(
          this.translateServices.instant('reset_password_success')
        )
      }
      else{
        this.toastr.error(
          this.translateServices.instant('reset_password_failure')
        )
      }
    })
    }
  }
}
