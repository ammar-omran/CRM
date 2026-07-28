import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UpdatePasswordInitComponent } from './update-password-init.component';

describe('UpdatePasswordInitComponent', () => {
  let component: UpdatePasswordInitComponent;
  let fixture: ComponentFixture<UpdatePasswordInitComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UpdatePasswordInitComponent]
    })
    .compileComponents();
    
    fixture = TestBed.createComponent(UpdatePasswordInitComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
