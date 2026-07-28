import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  template: `
    <div class="dashboard-container">
      <!-- Empty dashboard as requested -->
    </div>
  `,
  styles: [`
    .dashboard-container {
      padding: 20px;
      height: 100%;
    }
  `],
  standalone: true,
  imports: [CommonModule]
})
export class DashboardComponent implements OnInit {
  constructor() {}
  
  ngOnInit(): void {
    // Empty dashboard as requested
  }
}
