import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService } from './settings.service';
import { LanguageService } from '@shared/services';

@Injectable({
  providedIn: 'root',
})
export class TranslateLangService {
  constructor(
    private translate: TranslateService,
    private settings: SettingsService,
    private languageService: LanguageService
  ) { }

  load() {
    return new Promise<void>(resolve => {
      const browserLang = navigator.language;
      var defaultLang = this.languageService.isLanguageSupported(browserLang)
        ? browserLang
        : this.languageService.defaultLanguage;

      this.settings.setLanguage(defaultLang);
      this.translate.setDefaultLang(defaultLang);
      this.translate.use(defaultLang).subscribe({
        next: () => console.log(`Successfully initialized '${defaultLang}' language.'`),
        error: () => console.error(`Problem with '${defaultLang}' language initialization.'`),
        complete: () => resolve(),
      });
    });
  }
}
