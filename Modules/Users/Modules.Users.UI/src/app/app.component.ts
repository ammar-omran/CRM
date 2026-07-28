import { Component, OnInit, AfterViewInit } from '@angular/core';
import { PreloaderService, SettingsService } from '@core';
import { RouterOutlet } from '@angular/router';
import { FooterComponent } from './footer/footer.component';
import { LanguageService } from '@shared/services/language.service';
import { TranslateService } from '@ngx-translate/core';

@Component({
  selector: 'app-root',
  template: `
    <div class="content">
      <router-outlet></router-outlet>
      <app-footer></app-footer>
    </div>
  `,
  standalone: true,
  imports: [RouterOutlet, FooterComponent],
})
export class AppComponent implements OnInit, AfterViewInit {
  constructor(
    private preloader: PreloaderService,
    private settings: SettingsService,
    private languageService: LanguageService,
    private translateService: TranslateService
  ) {
    // Initialize language service
    this.initializeLanguage();
  }

  ngOnInit() {
    this.settings.setDirection();
    this.settings.setTheme();
  }

  ngAfterViewInit() {
    this.preloader.hide();
  }

  private initializeLanguage(): void {
    this.translateService.addLangs(['en-US', 'ar-EG']);
    this.translateService.setDefaultLang('en-US');
    this.languageService.getCurrentLanguage(); // This will trigger language setting
  }
}
