import { Component, OnInit, OnDestroy, AfterViewInit, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Title } from '@angular/platform-browser';
import * as L from 'leaflet';
import { CountryService, CountryDetails, CountryListItem, ObservedCountry, NeighbourData } from '../../services/country.service';
import { SectionRow } from '../../components/section-card/section-card.component';

@Component({
  selector: 'app-country',
  standalone: false,
  templateUrl: './country.component.html',
  styleUrl: './country.component.css'
})
export class CountryComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('mapContainer') mapContainer!: ElementRef;

  country: CountryDetails | null = null;
  loading = true;
  notFound = false;
  notObserved = false;
  error: string | null = null;
  cca3 = '';

  showModal = false;
  modalTitle = '';
  modalItems: string[] = [];

  showEditModal = false;
  showDeleteModal = false;
  currentCountry: ObservedCountry | null = null;

  addingNeighbour: string | null = null;

  private map: L.Map | null = null;
  private destroy$ = new Subject<void>();

  constructor(private route: ActivatedRoute, private router: Router, private countryService: CountryService, private titleService: Title) { }

  ngOnInit() {
    this.cca3 = this.route.snapshot.paramMap.get('code') ?? '';
    this.load();
  }

  ngAfterViewInit() { }

  ngOnDestroy() {
    this.map?.remove();
    this.destroy$.next();
    this.destroy$.complete();
  }

  load() {
    this.loading = true;
    this.notFound = false;
    this.notObserved = false;
    this.error = null;

    this.countryService.getObservedDetails(this.cca3)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (data) => {
          this.country = data;
          this.loading = false;
          this.titleService.setTitle(`Detailed Countries - ${data.commonName}`);
          setTimeout(() => this.initMap(), 100);
        },
        error: (err) => {
          this.loading = false;
          if(err.status === 404) {
            this.notFound = true;
          }
          else if(err.status === 403) {
            this.notObserved = true;
          }
          else {
            this.error = 'Failed to load country details.';
          }
        }
      });
  }

  initMap() {
    const el = this.mapContainer?.nativeElement;
    if(!el) {
      return;
    }

    if(this.country?.latitude == null || this.country?.longitude == null) {
      return;
    }

    if(this.map) {
      this.map.remove(); this.map = null;
    }

    this.map = L.map(el, { scrollWheelZoom: true })
      .setView([this.country.latitude, this.country.longitude], 4);

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
      attribution: '© OpenStreetMap contributors'
    }).addTo(this.map);

    this.loadBorders();
  }

  loadBorders() {
    fetch(`https://localhost:7156/data/countries/${this.cca3}.geo.json`)
      .then(r => {
        if(!r.ok) {
          return;
        }
        return r.json();
      })
      .then(feature => {
        if(!feature || !this.map) {
          return;
        }
        const layer = L.geoJSON(feature, {
          style: {
            color: '#4a9ebe',
            weight: 2,
            fillColor: '#cde8f2',
            fillOpacity: 0.35
          }
        }).addTo(this.map);

        setTimeout(() => {
          this.map!.invalidateSize();
          const bounds = layer.getBounds();
          if(bounds.isValid()) {
            this.map!.fitBounds(bounds, { padding: [20, 20] });
          }
        }, 100);
      })
      .catch(() => {
      });
  }

  get geographyRows(): SectionRow[] {
    if(!this.country) {
      return [];
    }
    return [
      { label: 'Capital', value: this.country.capital },
      { label: 'Continents', isArray: true, value: undefined, arrayValue: this.country.continents },
      { label: 'Region', value: this.country.region },
      { label: 'Subregion', value: this.country.subregion },
      { label: 'Area', value: this.country.area },
      { label: 'Landlocked', value: this.country.landlocked ? 'Yes' : 'No' },
      {
        label: 'Timezones',
        isArray: true,
        value: undefined,
        arrayValue: this.country.timezones.slice(0, 2),
        showMore: this.country.timezones.length > 2,
        onShowMore: () => this.openModal('Timezones', this.country!.timezones)
      }
    ];
  }

  get demographicsRows(): SectionRow[] {
    if(!this.country) {
      return [];
    }
    return [
      { label: 'Population', value: this.country.population },
      {
        label: 'Languages',
        isArray: true,
        value: undefined,
        arrayValue: this.country.languages.slice(0, 2),
        showMore: this.country.languages.length > 2,
        onShowMore: () => this.openModal('Languages', this.country!.languages)
      }
    ];
  }

  get politicalRows(): SectionRow[] {
    if(!this.country) {
      return [];
    }
    return [
      { label: 'Status', value: this.country.status },
      { label: 'UN Member', value: this.country.unMember ? 'Yes' : 'No' },
      { label: 'Independent', value: this.country.independent ? 'Yes' : 'No' }
    ];
  }

  get codesRows(): SectionRow[] {
    if(!this.country) {
      return [];
    }
    return [
      { label: 'CCA2', value: this.country.cca2 },
      { label: 'CCA3', value: this.country.cca3 },
      { label: 'CCN3', value: this.country.ccn3 },
      { label: 'CIOC', value: this.country.cioc },
      { label: 'Calling code', value: this.country.callingCode },
      {
        label: 'TLD',
        isArray: true,
        value: undefined,
        arrayValue: this.country.tld.slice(0, 2),
        showMore: this.country.tld.length > 2,
        onShowMore: () => this.openModal('Top Level Domains', this.country!.tld)
      }
    ];
  }

  get financesRows(): SectionRow[] {
    if(!this.country) {
      return [];
    }
    return [
      { label: 'Currency name', value: this.country.currencyName },
      { label: 'Currency code', value: this.country.currencyCode },
      { label: 'Currency symbol', value: this.country.currencySymbol },
      { label: 'Gini index', value: this.country.gini?.toString() }
    ];
  }

  openModal(title: string, items: string[]) {
    this.modalTitle = title;
    this.modalItems = items;
    this.showModal = true;
  }

  openNeighbour(n: NeighbourData) {
    if(n.isObserved) {
      window.location.href = `/country/${n.code}`;
      return;
    }
    if(!n.listItem || this.addingNeighbour) {
      return;
    }

    this.addingNeighbour = n.code;
    this.countryService.observe(n.listItem)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          n.isObserved = true;
          this.addingNeighbour = null;
        },
        error: () => {
          this.addingNeighbour = null;
        }
      });
  }

  observeThisCountry() {
    this.countryService.getAvailable()
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (available) => {
          const item = available.find(c => c.Code === this.cca3);
          if(!item) {
            return;
          }
          this.countryService.observe(item)
            .pipe(takeUntil(this.destroy$))
            .subscribe({
              next: () => window.location.reload(),
              error: () => this.error = 'Failed to observe country.'
            });
        }
      });
  }

  buildObservedCountry(): ObservedCountry {
    return {
      Id: '0',
      Code: this.country!.cca3!,
      Name: this.country!.commonName,
      OfficialName: this.country!.officialName,
      Flag: this.country!.flagSvg!,
      Alt: this.country!.flagAlt!,
    };
  }

  openEdit() {
    this.currentCountry = this.buildObservedCountry();
    this.showEditModal = true;
  }

  openDelete() {
    this.currentCountry = this.buildObservedCountry();
    this.showDeleteModal = true;
  }

  onCountryChanged(item: CountryListItem) {
    this.showEditModal = false;
    window.location.href = `/country/${item.Code}`;
  }

  onCountryDeleted(item: string) {
    this.showDeleteModal = false;
    this.router.navigate(['/countries']);
  }

  goToCountries() {
    this.router.navigate(['/countries']);
  }
}
