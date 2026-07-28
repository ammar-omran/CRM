import { Component, Input } from '@angular/core';
import { DynamicChartComponent } from '../../../shared/components/dynamic-chart/dynamic-chart.component';
import { DecimalPipe, NgClass } from '@angular/common';
import { MeterStatusDto, RetiredMeterStatusDto } from '@shared/interfaces/dashboard';

interface Stat {
  key: string;
  label: string;
  color: string;
}
@Component({
  selector: 'app-meter-stats-card',
  standalone: true,
  imports: [DynamicChartComponent, DecimalPipe, NgClass],
  templateUrl: './meter-stats-card.component.html',
  styleUrl: './meter-stats-card.component.scss',
})
export class MeterStatsCardComponent {
  @Input() title: string = '';
  @Input() type: string = '';
  @Input() total: number = 0;
  @Input() stats: Stat[] = [];
  @Input() values!: any;
  @Input() chartId: string = '';
  @Input() chartOptions: any;
}
