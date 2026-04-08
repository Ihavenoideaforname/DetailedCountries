import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { CountryService, CountryListItem } from '../../services/country.service';

@Component({
  selector: 'app-add-country-modal',
  standalone: false,
  templateUrl: './add-country-modal.component.html',
  styleUrl: './add-country-modal.component.css'
})
export class AddCountryModalComponent implements OnInit {
  @Output() closed = new EventEmitter<void>();
  @Output() countryAdded = new EventEmitter<CountryListItem>();

  countries: CountryListItem[] = [];
  searchQuery = '';
  selected: CountryListItem | null = null;
  loading = false;
  adding = false;
  error: string | null = null;
  addError: string | null = null;
  skeletonRows = Array(6).fill(0);

  constructor(private countryService: CountryService) { }

  ngOnInit() {
    this.loading = true;
    this.countryService.getAvailable().subscribe({
      next: (data) => {
        this.countries = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = err.status === 503
          ? 'Database unavailable. Try again later.'
          : 'Failed to load countries.';
        this.loading = false;
      }
    });
  }

  get filtered(): CountryListItem[] {
    const q = this.searchQuery.toLowerCase().trim();
    if(!q) return this.countries;
    return this.countries.filter(c =>
      c.Name.toLowerCase().includes(q)
    );
  }

  select(country: CountryListItem) {
    this.selected = this.selected?.Code === country.Code ? null : country;
    this.addError = null;
  }

  add() {
    if(!this.selected || this.adding) return;

    this.adding = true;
    this.addError = null;

    this.countryService.observe(this.selected).subscribe({
      next: () => {
        this.adding = false;
        this.countryAdded.emit(this.selected!);
        this.close();
      },
      error: (err) => {
        this.adding = false;
        this.addError = this.resolveAddError(err);
      }
    });
  }

  private resolveAddError(err: any): string {
    switch (err.status) {
      case 400: return 'Invalid country data.';
      case 409: return err.error ?? 'Country already in your collection.';
      case 502: return 'Could not verify country with external source.';
      case 504: return 'External source timed out. Try again.';
      case 503: return 'Database unavailable. Try again later.';
      default: return 'Something went wrong. Try again.';
    }
  }

  close() {
    this.closed.emit();
  }
}
