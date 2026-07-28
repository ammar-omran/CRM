import {
  AfterViewInit,
  Component,
  ContentChild,
  DestroyRef,
  EventEmitter,
  inject,
  Input,
  OnChanges,
  OnInit,
  Output,
  SimpleChanges,
  TemplateRef,
} from '@angular/core';
import { MatFormFieldModule } from '@angular/material/form-field';
import { PageEvent } from '@angular/material/paginator';
import { MtxGridColumn, MtxGridModule, MtxGridRowClassFormatter } from '@ng-matero/extensions/grid';
import { EndPoint, HttpVerb } from '@shared/enums';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { debounceTime, BehaviorSubject, Observable, switchMap, catchError, map } from 'rxjs';
import { ApiService } from '@shared/services/api.service';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-adaptive-table',
  standalone: true,
  imports: [MtxGridModule, MatFormFieldModule, AsyncPipe],
  templateUrl: './adaptive-table.component.html',
  styleUrl: './adaptive-table.component.scss',
})
export class AdaptiveTableComponent implements OnInit, OnChanges, AfterViewInit {
  @ContentChild('actionBtns') actionBtns!: TemplateRef<any>;
  @Input() apiUrl!: EndPoint;
  @Input() filters: any = {};
  @Input() columns: MtxGridColumn[] = [];
  @Input() isLoading: boolean = false;
  @Input() pageSizeOptions: number[] = [10, 50, 100];
  @Input() additionalParams: any = {};
  @Input() httpMethod: HttpVerb = HttpVerb.GET;
  @Input() rowClassFormatter: MtxGridRowClassFormatter = {};
  @Input() rowSelectable: boolean = true;
  @Input() rowHover: boolean = true;
  destroyRef = inject(DestroyRef);

  @Output() actionTriggered: EventEmitter<{ action: string; rowData: any }> = new EventEmitter<{
    action: string;
    rowData: any;
  }>();

  pageIndex = 0;
  pageSize = 10;
  totalRecords = 0;
  orderByField = '';
  sortOrder = '';
  data$!: Observable<any[]>;
  private fetchSubject = new BehaviorSubject<void>(undefined);

  constructor(private apiService: ApiService) {}

  ngOnInit(): void {
    this.data$ = this.fetchSubject.pipe(switchMap(() => this.fetchData()));
  }

  ngAfterViewInit(): void {
    this.assignTemplatesToColumns();
  }
  ngOnChanges(changes: SimpleChanges): void {
    if (changes.filters || changes.apiUrl || changes.additionalParams) {
      this.resetPagination();
      this.fetchSubject.next();
    }
  }

  sortChange(sortedColumns: any) {
    this.orderByField = sortedColumns.active;
    this.sortOrder = sortedColumns.direction;
    this.fetchSubject.next();
  }
  resetPagination(): void {
    this.pageIndex = 0;
    this.pageSize = 10;
    this.totalRecords = 0;
  }

  buildRequestParams() {
    return {
      limit: this.pageSize,
      skip: this.pageIndex * this.pageSize,
      sortColumn: this.orderByField,
      sortOrder: this.sortOrder,
      ...this.additionalParams,
      ...this.filters,
    };
  }

  fetchData(): Observable<any[]> {
    this.isLoading = true;
    return this.apiService
      .triggerApiRequest(this.apiUrl, this.httpMethod, this.buildRequestParams())
      .pipe(
        debounceTime(300),
        takeUntilDestroyed(this.destroyRef),
        switchMap((response: any) => {
          this.totalRecords = response.totalItemsCount;
          this.isLoading = false;
          return [response.data];
        }),
        catchError(err => {
          this.isLoading = false;

          return [];
        })
      );
  }

  onPageChange(event: PageEvent): void {
    this.pageIndex = event.pageIndex;
    this.pageSize = event.pageSize;
    this.fetchSubject.next();
  }

  private assignTemplatesToColumns() {
    if (this.actionBtns) {
      const actionsColumn = this.columns.find(col => col.header === 'Actions');
      if (actionsColumn) {
        actionsColumn.cellTemplate = this.actionBtns;
      }
    }
  }
}
