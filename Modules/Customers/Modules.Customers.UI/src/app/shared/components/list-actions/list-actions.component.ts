import { Component, EventEmitter, Input, Output } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-list-actions',
  standalone: true,
  imports: [MatIconModule, MatButtonModule],
  templateUrl: './list-actions.component.html',
  styleUrl: './list-actions.component.scss',
})
export class ListActionsComponent {
  @Input() showFilter: boolean = true;
  @Output() filterToggled = new EventEmitter<void>();

  filterOpen: boolean = false;

  onFilterClick(): void {
    this.filterOpen = !this.filterOpen;
    this.filterToggled.emit();
  }
}
