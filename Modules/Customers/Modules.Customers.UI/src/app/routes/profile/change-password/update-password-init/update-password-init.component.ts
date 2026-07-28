import { Component, OnInit } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from "@angular/material/icon";
import { Router } from '@angular/router';
import { TranslateModule, TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-update-password-init',
  standalone: true,
  imports: [
    MatButtonModule,
    MatCardModule,
    MatIconModule,
    TranslateModule
],
  templateUrl: './update-password-init.component.html',
  styleUrl: './update-password-init.component.scss'
})
export class UpdatePasswordInitComponent implements OnInit {
  constructor(
    private router: Router,
    private translateServices : TranslateService
  ){
    this.translateServices.setDefaultLang('en-US');
    this.translateServices.use(localStorage.getItem('lang') || 'en-US');
  }
  ngOnInit(): void {
  }
;
  changeByEmail(){
    this.router.navigate(['/changePassword/update-password']);
  }
  gotosignin(){
    this.router.navigate(['/auth/login']);
  }
}
