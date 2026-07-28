import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { TranslateService } from '@ngx-translate/core';
import { EndPoint } from '@shared/enums';

export interface EmailNotificationData {
  ticketId: number;
  customerEmail: string;
  customerName: string;
  ticketTitle: string;
  adminName: string;
  commentContent: string;
  ticketStatus: string;
  language?: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailNotificationService {

  constructor(
    private http: HttpClient,
    private translateService: TranslateService
  ) { }

  /**
   * Send email notification to customer when admin adds a comment
   * @param notificationData Data needed for the email notification
   * @returns Observable of the email sending result
   */
  sendCommentNotification(notificationData: EmailNotificationData): Observable<any> {
    const url = `${environment.ApiUrl}/${EndPoint.SEND_COMMENT_NOTIFICATION}`;

    // Get current language for email template
    const currentLang = this.translateService.currentLang || 'en';

    const payload = {
      ...notificationData,
      language: currentLang
    };

    return this.http.post(url, payload);
  }

  /**
   * Send email notification when ticket status changes
   * @param notificationData Data needed for the status change notification
   * @returns Observable of the email sending result
   */
  sendStatusChangeNotification(notificationData: EmailNotificationData): Observable<any> {
    const url = `${environment.ApiUrl}/${EndPoint.SEND_STATUS_CHANGE_NOTIFICATION}`;

    const currentLang = this.translateService.currentLang || 'en';

    const payload = {
      ...notificationData,
      language: currentLang
    };

    return this.http.post(url, payload);
  }

  /**
   * Send general ticket update notification
   * @param notificationData Data needed for the general update notification
   * @returns Observable of the email sending result
   */
  sendTicketUpdateNotification(notificationData: EmailNotificationData): Observable<any> {
    const url = `${environment.ApiUrl}/${EndPoint.SEND_TICKET_UPDATE_NOTIFICATION}`;

    const currentLang = this.translateService.currentLang || 'en';

    const payload = {
      ...notificationData,
      language: currentLang
    };

    return this.http.post(url, payload);
  }
}
