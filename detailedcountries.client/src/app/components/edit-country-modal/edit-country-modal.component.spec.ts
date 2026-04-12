import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditCountryModalComponent } from './edit-country-modal.component';

describe('EditCountryModalComponent', () => {
  let component: EditCountryModalComponent;
  let fixture: ComponentFixture<EditCountryModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [EditCountryModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditCountryModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
