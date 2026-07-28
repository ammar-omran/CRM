import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { PageHeaderComponent } from '@shared';
import { MatTableModule } from '@angular/material/table';
import { MatTabGroup, MatTab } from '@angular/material/tabs';
import { TicketHistoryComponent } from '../ticket-history/ticket-history.component';
import { MatCard, MatCardContent, MatCardTitle } from '@angular/material/card';
import { EndPoint, HttpVerb } from '@shared/enums';
import { TicketDetails } from '@shared/interfaces/ticket-details';
import { HelperService } from '@shared/services/helper.service';
import { ApiService } from '@shared/services/api.service';
import { TranslateModule } from '@ngx-translate/core';
import { TranslateService } from '@ngx-translate/core';
import { MatButtonToggleGroup, MatButtonToggle } from '@angular/material/button-toggle';
import { MatButtonModule } from '@angular/material/button';
import { CommentComponent } from '@shared/utils/comment/comment.component';
import { AddCommentComponent } from '../add-comment/add-comment.component';
import { HasPermissionDirective } from '@shared';
@Component({
  standalone: true,
  selector: 'app-ticket-details',
  templateUrl: './ticket-details.component.html',
  styleUrls: ['./ticket-details.component.scss'],
  imports: [
    CommonModule,
    PageHeaderComponent,
    MatTableModule,
    MatTabGroup,
    MatTab,
    TicketHistoryComponent,
    MatCard,
    MatCardContent,
    MatCardTitle,
    TranslateModule,
    MatButtonToggleGroup,
    MatButtonToggle,
    MatButtonModule,
    CommentComponent,
    AddCommentComponent,
    HasPermissionDirective,
  ],
})
export class TicketDetailsComponent implements OnInit {
  ticketId!: number;
  showHistoryTab = false;

  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private apiService = inject(ApiService);
  private translate = inject(TranslateService);
  currentLang: string = 'en-US';
  setLang(lang: string) {
    this.currentLang = lang;
    this.translate.use(lang);
    localStorage.setItem('lang', lang);
  }
  ticket: TicketDetails | null = null;
  error: string | null = null;
  loading = true;

  ngOnInit(): void {
    const savedLang = localStorage.getItem('lang') || 'en-US';
    this.currentLang = savedLang;
    this.translate.setDefaultLang('en-US');
    this.translate.use(savedLang);
    const id = parseInt(this.route.snapshot.params.ticketId, 10);
    if (!id) {
      return;
    }
    this.ticketId = id;
    this.loadTicketDetails();
  }

  private loadTicketDetails(): void {
    const endpoint = HelperService.formatEndpoint(EndPoint.GET_TICKET_DETAILS, {
      ticketId: this.ticketId,
    }) as EndPoint;
    this.getComments();
    this.apiService.triggerApiRequest<TicketDetails>(endpoint, HttpVerb.GET).subscribe({
      next: data => {
        this.ticket = data;
        this.loading = false;
      },
      error: err => {
        this.error = 'Failed to load ticket details.';
        this.loading = false;
        console.error('API error:', err);
      },
    });
  }

  // Method to refresh ticket details (can be called after comment is added)
  refreshTicketDetails(): void {
    this.loadTicketDetails();
  }

  onTabChange(event: any): void {
    if (event.index === 1) {
      this.showHistoryTab = true;
    }
  }

  navigateToAddComment(): void {
    this.router.navigate(['/tickets', this.ticketId, 'add-comment']);
  }
  ticketComments: any[] = [];
  adminId: string = localStorage.getItem('adminId') || 'no';
  getComments(){
    this.apiService.triggerApiRequest(
       `${EndPoint.GET_TICKET_COMMENTS}/${this.ticketId}` as EndPoint,
       HttpVerb.GET,
      ).subscribe((response: any) => {
        if(response.status){
          if(response.ticketComments != null){
            this.ticketComments = response.ticketComments.map((comment: any) => {
              console.log(this.adminId);
              if(String(comment.createdById) == '1'){
                return {
                author: comment.createdByName,
                description: String(comment.description),
                createdDate: String(comment.createdDate),
                role: 'Support'
              };
              }
              else{
                return {
                author: comment.createdByName,
                description: comment.description,
                createdDate: comment.createdDate ,
                role: 'Customer'
              };
              }
            }
          );
          }
        }
    });
  }
  handleCommentAdded(){
    this.getComments();
  }
}
