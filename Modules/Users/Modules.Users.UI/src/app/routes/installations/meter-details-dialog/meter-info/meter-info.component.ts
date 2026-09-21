import { CommonModule } from '@angular/common';
import { Component, Input, OnInit } from '@angular/core';

export interface MeterDetail {
  label: string;
  value: string | number | null;
}
@Component({
  selector: 'app-meter-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './meter-info.component.html',
  styleUrl: './meter-info.component.scss',
})
export class MeterInfoComponent {
  @Input() details: MeterDetail[] = [];
  @Input() meterStatus: string = '';
}
