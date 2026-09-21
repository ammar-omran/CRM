import { Component, inject } from '@angular/core';
import { FilterComponent } from '../../../shared/components/filter/filter.component';
import { AdaptiveTableComponent } from '../../../shared/components/adaptive-table/adaptive-table.component';
import { EndPoint, HttpVerb } from '@shared/enums';
import { FilterControl } from '@shared/interfaces/filter-control.model';
import { MtxGridColumn } from '@ng-matero/extensions/grid';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { AgentResponse } from '@shared/interfaces/agent.model';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ListActionsComponent } from '@shared/components/list-actions/list-actions.component';
import { MatMenuModule } from '@angular/material/menu';

@Component({
  selector: 'app-agents',
  standalone: true,
  imports: [
    FilterComponent,
    AdaptiveTableComponent,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    ListActionsComponent,
    MatMenuModule,
  ],
  templateUrl: './agents.component.html',
  styleUrl: './agents.component.scss',
})
export class AgentsComponent {
  filterVisible = false;
  toggleFilter(): void {
    this.filterVisible = !this.filterVisible;
  }

  // Backend AgentResponse: { id, userId, name, email } — paginated via ?UserId=&Skip=&Limit=
  filters: Record<string, string> = {};
  columns: MtxGridColumn<AgentResponse>[] = [
    { header: 'ID', field: 'id', sortable: true },
    { header: 'Name', field: 'name', sortable: true },
    { header: 'Email', field: 'email', sortable: true },
    { header: 'User ID', field: 'userId' },
    { header: 'Actions', field: 'actions' as keyof AgentResponse, width: '170px' },
  ];

  filterControls: FilterControl[] = [
    {
      formControlName: 'UserId',
      label: 'Search By User ID',
      type: 'text',
      optionLabel: 'User ID',
      optionVal: 'UserId',
    },
  ];

  httpVerb: HttpVerb = HttpVerb.GET;
  endpoint = EndPoint.AGENTS_LIST; // GET api/agents — YARP gateway, no contractor scoping

  onFilterChanged(filterValues: Record<string, string>): void {
    this.filters = filterValues;
  }
}
