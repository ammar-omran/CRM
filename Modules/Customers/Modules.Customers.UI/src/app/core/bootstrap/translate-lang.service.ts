import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { SettingsService } from './settings.service';

@Injectable({
  providedIn: 'root',
})
export class TranslateLangService {
  constructor(
    private translate: TranslateService,
    private settings: SettingsService
  ) { }

  load() {
    return new Promise<void>(resolve => {
      const browserLang = navigator.language;
      const defaultLang = browserLang.match(/en-US|zh-CN|zh-TW|ar-EG/) ? browserLang : 'en-US';

      this.settings.options.dir = defaultLang.startsWith('ar') ? 'rtl' : 'ltr';
      this.settings.setDirection();
      this.settings.setLanguage(defaultLang);
      this.translate.setDefaultLang(defaultLang);
      this.translate.use(defaultLang).subscribe({
        next: () => console.log(`Successfully initialized '${defaultLang}' language.'`),
        error: () => console.error(`Problem with '${defaultLang}' language initialization.'`),
        complete: () => resolve(),
      });
    });
  }

  supportedLangs = ['en-US', 'zh-CN', 'zh-TW', 'ar-EG'];

  switchLang(lang: string) {
    if (!this.supportedLangs.includes(lang)) lang = this.supportedLangs[0];
    if (lang == this.settings.options.language) return;

    return new Promise<void>(resolve => {
      this.settings.setLanguage(lang);
      this.translate.setDefaultLang(lang);
      this.translate.use(lang).subscribe({
        complete: () => resolve(),
      });
    });
  }
}
