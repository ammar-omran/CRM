import { Component } from '@angular/core';

@Component({
  selector: 'app-branding',
  template: `
    <a class="d-inline-block" href="/">
      <img src="./assets/images/svgs/panel-logo.svg" alt="Ghana Electrometers logo" />
    </a>
  `,
  styles: [
    `
      .brand-logo {
        width: 30px;
        height: 30px;
      }

      a {
        text-decoration: none;
      }
    `,
  ],
  standalone: true,
})
export class BrandingComponent {}
