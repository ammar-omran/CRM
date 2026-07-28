import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '@env/environment';
import { ApiService } from './api.service';
import { EndPoint, HttpVerb } from '@shared/enums';
import { EMAIL_CONFIG } from '../config/email.config';

export interface TicketEmailNotification {
  customerEmail: string;
  customerName: string;
  ticketId: number;
  ticketReference: string;
  subject: string;
  description: string;
  category: string;
  service: string;
  type: string;
  severity: string;
  creationDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class EmailNotificationService {
  private readonly emailApiUrl = `${environment.ApiUrl}/notifications/send-email`;

  constructor(
    private http: HttpClient,
    private apiService: ApiService
  ) {}

  /**
   * Send ticket creation confirmation email to customer
   */
  sendTicketCreationEmail(notification: TicketEmailNotification): Observable<any> {
    const emailData = {
      to: notification.customerEmail,
      subject: EMAIL_CONFIG.SUBJECTS.TICKET_CREATED.replace('{ticketReference}', notification.ticketReference),
      template: EMAIL_CONFIG.TEMPLATES.TICKET_CREATION,
      data: {
        customerName: notification.customerName,
        ticketId: notification.ticketId,
        ticketReference: notification.ticketReference,
        subject: notification.subject,
        description: notification.description,
        category: notification.category,
        service: notification.service,
        type: notification.type,
        severity: notification.severity,
        creationDate: notification.creationDate,
        supportEmail: EMAIL_CONFIG.SUPPORT_EMAIL,
        supportPhone: EMAIL_CONFIG.SUPPORT_PHONE
      }
    };

    return this.http.post(this.emailApiUrl, emailData);
  }

  /**
   * Send ticket creation confirmation email using the existing API service pattern
   */
  sendTicketCreationEmailViaApiService(notification: TicketEmailNotification): Observable<any> {
    const emailData = {
      to: notification.customerEmail,
      subject: EMAIL_CONFIG.SUBJECTS.TICKET_CREATED.replace('{ticketReference}', notification.ticketReference),
      template: EMAIL_CONFIG.TEMPLATES.TICKET_CREATION,
      data: notification
    };

    return this.apiService.triggerApiRequest(
      EndPoint.SEND_TICKET_EMAIL_NOTIFICATION,
      HttpVerb.POST,
      undefined,
      emailData
    );
  }

  /**
   * Generate a professional email template for ticket creation
   */
  private generateEmailTemplate(notification: TicketEmailNotification): string {
    return `
      <!DOCTYPE html>
      <html>
      <head>
        <meta charset="utf-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Ticket Created Successfully</title>
        <style>
          body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
          .container { max-width: 600px; margin: 0 auto; padding: 20px; }
          .header { background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
          .content { background: #f8f9fa; padding: 20px; border-radius: 0 0 5px 5px; }
          .ticket-info { background: white; padding: 15px; margin: 15px 0; border-radius: 5px; border-left: 4px solid #007bff; }
          .footer { text-align: center; margin-top: 20px; color: #666; font-size: 12px; }
          .highlight { color: #007bff; font-weight: bold; }
        </style>
      </head>
      <body>
        <div class="container">
          <div class="header">
            <h1>🎫 Ticket Created Successfully</h1>
          </div>
          
          <div class="content">
            <p>Dear <span class="highlight">${notification.customerName}</span>,</p>
            
            <p>Thank you for contacting our support team. Your ticket has been created successfully and is now being processed.</p>
            
            <div class="ticket-info">
              <h3>Ticket Details:</h3>
              <p><strong>Ticket Reference:</strong> <span class="highlight">${notification.ticketReference}</span></p>
              <p><strong>Ticket ID:</strong> ${notification.ticketId}</p>
              <p><strong>Subject:</strong> ${notification.subject}</p>
              <p><strong>Category:</strong> ${notification.category}</p>
              <p><strong>Service:</strong> ${notification.service}</p>
              <p><strong>Type:</strong> ${notification.type}</p>
              <p><strong>Severity:</strong> ${notification.severity}</p>
              <p><strong>Created:</strong> ${notification.creationDate}</p>
            </div>
            
            <p><strong>Description:</strong></p>
            <p style="background: white; padding: 10px; border-radius: 3px; border: 1px solid #ddd;">${notification.description}</p>
            
            <p>Our support team will review your request and get back to you as soon as possible. You can track the progress of your ticket using the ticket reference number above.</p>
            
            <p>If you have any urgent questions, please don't hesitate to contact us:</p>
            <ul>
              <li>📧 Email: <a href="mailto:${EMAIL_CONFIG.SUPPORT_EMAIL}">${EMAIL_CONFIG.SUPPORT_EMAIL}</a></li>
              <li>📞 Phone: ${EMAIL_CONFIG.SUPPORT_PHONE}</li>
            </ul>
            
            <p>Thank you for your patience and understanding.</p>
            
            <p>Best regards,<br>The ${EMAIL_CONFIG.SETTINGS.FROM_NAME}</p>
          </div>
          
          <div class="footer">
            <p>This is an automated message. Please do not reply to this email.</p>
            <p>© 2025 ${EMAIL_CONFIG.COMPANY_NAME}. All rights reserved.</p>
          </div>
        </div>
      </body>
      </html>
    `;
  }
}
