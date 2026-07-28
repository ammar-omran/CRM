import { Component, ViewEncapsulation } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { environment } from '@env/environment';

@Component({
  selector: 'app-auth-layout',
  templateUrl: './auth-layout.component.html',
  styleUrls: ['./auth-layout.component.scss'],
  standalone: true,
  imports: [RouterOutlet],
})
export class AuthLayoutComponent {
  version = environment.versionNumber;
  releaseDate = environment.releaseDate;
}
