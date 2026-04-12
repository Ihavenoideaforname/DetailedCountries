import { ComponentFixture, TestBed } from '@angular/core/testing';

import { RemoveCountryModalComponent } from './remove-country-modal.component';

describe('RemoveCountryModalComponent', () => {
  let component: RemoveCountryModalComponent;
  let fixture: ComponentFixture<RemoveCountryModalComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [RemoveCountryModalComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(RemoveCountryModalComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
