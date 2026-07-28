import { ComponentFixture, TestBed } from '@angular/core/testing';

import { TicketHistoryRecordComponent } from './ticket-history-record.component';

describe('TicketHistoryRecordComponent', () => {
  let component: TicketHistoryRecordComponent;
  let fixture: ComponentFixture<TicketHistoryRecordComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TicketHistoryRecordComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(TicketHistoryRecordComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
