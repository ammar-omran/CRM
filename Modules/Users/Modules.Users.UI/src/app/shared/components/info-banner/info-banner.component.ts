import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { animate, style, transition, trigger } from '@angular/animations';

export type BannerType = 'info' | 'warning' | 'error' | 'success';

@Component({
  selector: 'app-info-banner',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatIconModule],
  templateUrl: './info-banner.component.html',
  styleUrl: './info-banner.component.scss',
  animations: [
    trigger('slideDown', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(-8px)', maxHeight: '0px' }),
        animate(
          '250ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({ opacity: 1, transform: 'translateY(0)', maxHeight: '300px' })
        ),
      ]),
      transition(':leave', [
        animate(
          '180ms cubic-bezier(0.4, 0, 0.2, 1)',
          style({ opacity: 0, transform: 'translateY(-8px)', maxHeight: '0px' })
        ),
      ]),
    ]),
  ],
})
export class InfoBannerComponent {
  @Input() type: BannerType = 'info';
  @Input() title = '';
  @Input() message = '';
  @Input() confirmLabel = '';
  @Input() confirmIcon = 'add';
  @Input() cancelLabel = '';

  @Output() confirmed = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  readonly iconMap: Record<BannerType, string> = {
    info: 'info',
    warning: 'warning',
    error: 'error',
    success: 'check_circle',
  };

  onConfirm(): void {
    this.confirmed.emit();
  }
  onCancel(): void {
    this.cancelled.emit();
  }
}
