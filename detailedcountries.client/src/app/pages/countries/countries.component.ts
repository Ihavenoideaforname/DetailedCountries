import { Component, OnInit, OnDestroy, HostListener, AfterViewInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { count, takeUntil } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import { CountryService, CountryListItem, ObservedCountry } from '../../services/country.service';

@Component({
  selector: 'app-countries',
  standalone: false,
  templateUrl: './countries.component.html',
  styleUrl: './countries.component.css'
})
export class CountriesComponent implements OnInit, AfterViewInit, OnDestroy {
  allCountries: ObservedCountry[] = [];
  pagedCountries: ObservedCountry[] = [];

  loading = false;
  error: string | null = null;
  searchQuery = '';
  sortOrder: 'asc' | 'desc' = 'asc';

  currentPage = 1;
  pageSize = 12;
  totalPages = 0;

  showAddModal = false;
  showEditModal = false;
  showDeleteModal = false;
  selectedCountry: ObservedCountry | null = null;

  skeletonCards = Array(12).fill(0);

  private destroy$ = new Subject<void>();

  constructor(private countryService: CountryService, private router: Router, private titleService: Title) { }

  title = 'Detailed Countries - Countries';

  ngOnInit() {
    this.titleService.setTitle(this.title);
    this.load();
  }

  ngAfterViewInit() {
    setTimeout(() => this.recalculatePageSize(), 0);
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  @HostListener('window:resize')
  recalculatePageSize() {
    const cardW = 250 + 16;
    const cardH = 160 + 16;
    const availW = window.innerWidth - 64;
    const availH = window.innerHeight - 180;

    const cols = Math.max(1, Math.floor(availW / cardW));
    const rows = Math.max(2, Math.floor(availH / cardH));

    this.pageSize = cols * rows;
    this.skeletonCards = Array(this.pageSize).fill(0);
    this.applyFilters();
  }

  load() {
    this.loading = true;
    this.error = null;

    this.countryService.getObserved().pipe(takeUntil(this.destroy$)).subscribe({
      next: (countries) => {
        this.allCountries = countries;
        this.loading = false;
        this.applyFilters();
      },
      error: (err) => {
        console.error('Error loading countries:', err);
        this.error = 'Failed to load countries.';
        this.loading = false;
      }
    });
  }

  onSearch(query: string) {
    this.searchQuery = query;
    this.currentPage = 1;
    this.applyFilters();
  }

  toggleSort() {
    this.sortOrder = this.sortOrder === 'asc' ? 'desc' : 'asc';
    this.applyFilters();
  }

  applyFilters() {
    const q = this.searchQuery.toLowerCase().trim();

    let result = this.allCountries.filter(c =>
      !q ||
      c.Name.toLowerCase().includes(q) ||
      c.OfficialName.toLowerCase().includes(q) ||
      c.Code.toLowerCase().includes(q)
    );

    result = [...result].sort((a, b) => {
      const cmp = a.Name.localeCompare(b.Name);
      return this.sortOrder === 'asc' ? cmp : -cmp;
    });

    this.totalPages = Math.max(1, Math.ceil(result.length / this.pageSize));

    if(this.currentPage > this.totalPages) {
      this.currentPage = this.totalPages;
    }

    const start = (this.currentPage - 1) * this.pageSize;
    this.pagedCountries = result.slice(start, start + this.pageSize);
  }

  goToPage(page: number) {
    if(page < 1 || page > this.totalPages) {
      return;
    }
    this.currentPage = page;
    this.applyFilters();
  }

  get pages(): number[] {
    const range = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);

    for(let i = start; i <= end; i++) {
      range.push(i);
    }

    return range;
  }

  openDelete(country: ObservedCountry) {
    this.selectedCountry = country;
    this.showDeleteModal = true;
  }

  openEdit(country: ObservedCountry) {
    this.selectedCountry = country;
    this.showEditModal = true;
  }

  explore(country: ObservedCountry) {
    this.router.navigate(['/country', country.Code]);
  }

  onCountryAdded(country: CountryListItem) {
    console.log('Added: ', country);
    this.countryService.getObservedByCode(country.Code)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (observed) => {
          this.allCountries.push(observed);
          this.showAddModal = false;
          this.applyFilters();
        },
        error: (err) => {
          console.log('Error fetching observed country after add:', err);
          this.showAddModal = false;
          this.error = 'Country added but failed to refresh. Please reload.';
        }
      });
  }

  onCountryChanged(country: CountryListItem) {
    console.log('Changed: ', country);
    const oldCode = this.selectedCountry!.Code;
    this.countryService.getObservedByCode(country.Code)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (observed) => {
          const index = this.allCountries.findIndex(c => c.Code === oldCode);

          if (index !== -1) {
            this.allCountries[index] = observed;
            this.applyFilters();
          }

          this.showEditModal = false;
          this.selectedCountry = null;
        },
        error: (err) => {
          console.log('Error fetching observed country after edit:', err);
          this.showEditModal = false;
          this.selectedCountry = null;
          this.error = 'Country updated but failed to refresh. Please reload.';
        }
      });
  }

  onCountryDeleted(code: string) {
    console.log('Deleted: ', code);
    this.allCountries = this.allCountries.filter(c => c.Code !== code);
    this.showDeleteModal = false;
    this.selectedCountry = null;
    this.applyFilters();
  }
}
