import { Component, ViewEncapsulation } from '@angular/core';
import { RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-change-password-layout',
  standalone: true,
  imports: [RouterOutlet],
  templateUrl: './change-password-layout.component.html',
  styleUrl: './change-password-layout.component.scss',
  encapsulation: ViewEncapsulation.None,
})
export class ChangePasswordLayoutComponent {

}
