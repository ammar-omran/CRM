import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { environment } from '@env/environment';
import { TranslateService } from '@ngx-translate/core';

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
   * Email notification endpoints removed (previous legacy endpoints deleted).
   * These methods are now stubs – notifications should be handled server-side.
   */
  sendCommentNotification(_notificationData: EmailNotificationData): Observable<any> {
    return throwError(() => new Error('Email notification endpoint removed'));
  }

  sendStatusChangeNotification(_notificationData: EmailNotificationData): Observable<any> {
    return throwError(() => new Error('Email notification endpoint removed'));
  }

  sendTicketUpdateNotification(_notificationData: EmailNotificationData): Observable<any> {
    return throwError(() => new Error('Email notification endpoint removed'));
  }
}
