import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MeterOperationsComponent } from './meter-operations.component';

describe('MeterOperationsComponent', () => {
  let component: MeterOperationsComponent;
  let fixture: ComponentFixture<MeterOperationsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MeterOperationsComponent],
    }).compileComponents();

    fixture = TestBed.createComponent(MeterOperationsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
