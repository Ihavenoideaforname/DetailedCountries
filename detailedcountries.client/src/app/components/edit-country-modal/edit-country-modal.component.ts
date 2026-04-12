import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { CountryService, CountryListItem, ObservedCountry } from '../../services/country.service';

@Component({
  selector: 'app-edit-country-modal',
  standalone: false,
  templateUrl: './edit-country-modal.component.html',
  styleUrl: './edit-country-modal.component.css'
})
export class EditCountryModalComponent implements OnInit {
  @Input() country!: ObservedCountry;
  @Output() closed = new EventEmitter<void>();
  @Output() countryEdited = new EventEmitter<CountryListItem>();

  countries: CountryListItem[] = [];
  searchQuery = '';
  selected: CountryListItem | null = null;
  loading = false;
  editing = false;
  error: string | null = null;
  editError: string | null = null;
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
    this.editError = null;
  }

  edit() {
    if(!this.selected || this.editing) return;

    this.editing = true;
    this.editError = null;

    this.countryService.editObserved(this.country.Code, this.selected).subscribe({
      next: () => {
        this.editing = false;
        this.countryEdited.emit(this.selected!);
        this.close();
      },
      error: (err) => {
        this.editing = false;
        this.editError = this.resolveEditError(err);
      }
    })
  }

  private resolveEditError(err: any): string {
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
