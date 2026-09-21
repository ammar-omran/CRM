import { NgClass } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { Agent } from '@shared/interfaces/agent.model';

@Component({
  selector: 'app-agent-details-dialog',
  standalone: true,
  imports: [MatDialogModule, NgClass, MatIconModule, MatButtonModule],
  templateUrl: './agent-details-dialog.component.html',
  styleUrl: './agent-details-dialog.component.scss',
})
export class AgentDetailsDialogComponent {
  noData = 'N/A';
  constructor(
    public dialogRef: MatDialogRef<AgentDetailsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { agent: Agent }
  ) {}

  closeDialog(): void {
    this.dialogRef.close();
  }
}
