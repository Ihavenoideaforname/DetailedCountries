import { Component, Input, Output, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';
import { CountryService, ObservedCountry } from '../../services/country.service'; 

@Component({
  selector: 'app-remove-country-modal',
  standalone: false,
  templateUrl: './remove-country-modal.component.html',
  styleUrl: './remove-country-modal.component.css'
})
export class RemoveCountryModalComponent {
  @Input() country!: ObservedCountry;
  @Output() closed = new EventEmitter<void>();
  @Output() countryDeleted = new EventEmitter<string>();

  deleting = false;
  deleteError: string | null = null;

  constructor(private router: Router, private countryService: CountryService) { }

  delete() {
    if(this.deleting) {
      return;
    }
    this.deleting = true;
    this.deleteError = null;
    this.countryService.removeObserved(this.country.Code).subscribe({
      next: () => {
        this.deleting = false;
        this.countryDeleted.emit(this.country.Code);
        this.close();
      },
      error: (err) => {
        if(err.status === 503) {
          this.redirectToError(503, 'Database unavailable. Try again later.')
        }
        else {
          this.redirectToError(500, 'Failed to delete country.')
        }
      }
    });
  }

  private redirectToError(status: number, message: string) {
    this.router.navigate(['/error'], { state: { status, message } });
  }

  close() {
    this.closed.emit();
  }
}
