import { Injectable } from '@angular/core';
import { TranslateService } from '@ngx-translate/core';
import { BehaviorSubject } from 'rxjs';

export interface Language {
  code: string;
  name: string;
  rtl: boolean;
}

export const supportedLanguages: Language[] = [
  { code: 'en-US', name: 'English', rtl: false },
  { code: 'ar-EG', name: 'العربية', rtl: true }  
];

@Injectable({
  providedIn: 'root'
})
export class LanguageService {
  private currentLanguageSubject = new BehaviorSubject<string>('en-US');
  public currentLanguage$ = this.currentLanguageSubject.asObservable();
  public defaultLanguage = 'en-US';

  constructor(private translateService: TranslateService) {
    this.initializeLanguage();
  }

  private initializeLanguage(): void {
    // Get saved language from localStorage or use browser language
    const savedLanguage = localStorage.getItem('preferred-language');
    const browserLanguage = navigator.language.split('-')[0];
    
    let languageToUse = 'en-US'; // default
    
    if (savedLanguage && this.isLanguageSupported(savedLanguage)) {
      languageToUse = savedLanguage;
    } else if (this.isLanguageSupported(browserLanguage)) {
      languageToUse = browserLanguage;
    }

    this.setLanguage(languageToUse);
  }

  public getSupportedLanguages(): Language[] {
    return supportedLanguages;
  }

  public getCurrentLanguage(): string {
    return this.currentLanguageSubject.value;
  }

  public setLanguage(languageCode: string): void {
    if (!this.isLanguageSupported(languageCode)) {
      console.warn(`Language ${languageCode} is not supported`);
      return;
    }

    // Set the language in translate service
    this.translateService.use(languageCode);

    // Update current language subject
    this.currentLanguageSubject.next(languageCode);

    // Save to localStorage
    localStorage.setItem('preferred-language', languageCode);

    // Update document direction and language
    const language = supportedLanguages.find(lang => lang.code === languageCode);
    if (language) {
      document.documentElement.dir = language.rtl ? 'rtl' : 'ltr';
      document.documentElement.lang = languageCode;

      // Add RTL class to body for additional styling
      if (language.rtl) {
        document.body.classList.add('rtl');
        document.body.classList.remove('ltr');
      } else {
        document.body.classList.add('ltr');
        document.body.classList.remove('rtl');
      }
    }
  }

  public toggleLanguage(): void {
    const currentLang = this.getCurrentLanguage();
    const nextLang = currentLang === 'en' ? 'ar' : 'en';
    this.setLanguage(nextLang);
  }

  public isLanguageSupported(languageCode: string): boolean {
    return supportedLanguages.some(lang => lang.code === languageCode);
  }

  public isRTL(): boolean {
    const currentLang = this.getCurrentLanguage();
    const language = supportedLanguages.find(lang => lang.code === currentLang);
    return language ?.rtl || false;
  }

  public getLanguageName(languageCode: string): string {
    const language = supportedLanguages.find(lang => lang.code === languageCode);
    return language ?.name || languageCode;
  }
}
