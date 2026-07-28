import {
  AfterViewInit,
  Component,
  Input,
  OnChanges,
  OnDestroy,
  SimpleChanges,
} from '@angular/core';

@Component({
  selector: 'app-dynamic-chart',
  standalone: true,
  imports: [],
  templateUrl: './dynamic-chart.component.html',
  styleUrl: './dynamic-chart.component.scss',
})
export class DynamicChartComponent implements OnChanges, AfterViewInit, OnDestroy {
  @Input() chartId!: string;
  @Input() chartOptions!: any;

  private chart!: any;

  ngAfterViewInit(): void {
    if (this.chartOptions && this.chartId) {
      this.initializeChart();
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.chartOptions && !changes.chartOptions.firstChange) {
      this.destroyChart();
      this.initializeChart();
    }
  }

  private initializeChart(): void {
    const chartElement = document.querySelector(`#${this.chartId}`);
    if (chartElement) {
      this.chart = new ApexCharts(chartElement, this.chartOptions);
      this.chart.render();
    }
  }

  private destroyChart(): void {
    if (this.chart) {
      this.chart.destroy();
    }
  }
  ngOnDestroy(): void {
    this.destroyChart();
  }
}
