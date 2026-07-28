import { Component, EventEmitter, inject, Input, Output, ViewEncapsulation } from '@angular/core';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';

import { SidemenuComponent } from '../sidemenu/sidemenu.component';
import { BrandingComponent } from '../widgets/branding.component';
import { UserPanelComponent } from './user-panel.component';
import { RouterLink } from '@angular/router';
import { admin } from '@core';
import { MatButton } from '@angular/material/button';

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss'],
  encapsulation: ViewEncapsulation.None,
  standalone: true,
  imports: [
    RouterLink,
    MatSlideToggleModule,
    BrandingComponent,
    SidemenuComponent,
    UserPanelComponent,
    MatButton,
  ],
})
export class SidebarComponent {
  @Input() showToggle = true;
  @Input() showUser = false;
  @Input() showHeader = true;
  @Input() toggleChecked = false;

  @Output() toggleCollapsed = new EventEmitter<void>();
  adminId = admin.id;
}
