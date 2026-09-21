import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MeterItem } from '@shared/interfaces/meter-info.model';

@Component({
  selector: 'app-spare-parts-table',
  standalone: true,
  imports: [MatTableModule],
  templateUrl: './spare-parts-table.component.html',
  styleUrl: './spare-parts-table.component.scss',
})
export class SparePartsTableComponent implements OnChanges {
  @Input() spareParts: MeterItem[] = [];

  displayedColumns: string[] = ['index', 'name', 'quantity'];
  dataSource = new MatTableDataSource<MeterItem>();

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.spareParts) {
      this.dataSource.data = this.spareParts.map((item, index) => ({
        ...item,
        index: index + 1,
      }));
    }
  }
}
