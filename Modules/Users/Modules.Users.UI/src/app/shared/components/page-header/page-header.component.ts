import { BooleanInput, coerceBooleanProperty } from '@angular/cdk/coercion';
import { Component, HostBinding, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';

import { MenuService } from '@core';
import { BreadcrumbComponent } from '../breadcrumb/breadcrumb.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'page-header',
  templateUrl: './page-header.component.html',
  styleUrls: ['./page-header.component.scss'],
  standalone: true,
  imports: [
    BreadcrumbComponent,
    TranslateModule,
    MatTooltipModule,
    RouterModule,
    MatIconModule,
    MatButtonModule,
  ],
})
export class PageHeaderComponent implements OnInit {
  @HostBinding('class') class = 'matero-page-header';

  @Input() title = '';
  @Input() subtitle = '';
  @Input() addLink?: string;
  @Input() addTooltip: string = 'Add';
  @Input() addIcon: string = 'add';

  @Input() nav: string[] = [];
  @Input()
  get hideBreadcrumb() {
    return this._hideBreadCrumb;
  }
  set hideBreadcrumb(value: boolean) {
    this._hideBreadCrumb = coerceBooleanProperty(value);
  }
  private _hideBreadCrumb = true;

  constructor(
    private router: Router,
    private menu: MenuService
  ) {}

  ngOnInit() {
    this.nav = Array.isArray(this.nav) ? this.nav : [];

    if (this.nav.length === 0) {
      this.genBreadcrumb();
    }

    this.title = this.title || this.nav[this.nav.length - 1];
  }

  genBreadcrumb() {
    const routes = this.router.url.slice(1).split('/');
    this.nav = this.menu.getLevel(routes);
    this.nav.unshift('home');
  }

  static ngAcceptInputType_hideBreadcrumb: BooleanInput;
}
