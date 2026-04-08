import { ComponentFixture, TestBed } from '@angular/core/testing';

import { AddCountryModalComponent } from './add-country-modal.component';

describe('AddCountryModalComponent', () => {
  let component: AddCountryModalComponent;
  let fixture: ComponentFixture<AddCountryModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [AddCountryModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(AddCountryModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
