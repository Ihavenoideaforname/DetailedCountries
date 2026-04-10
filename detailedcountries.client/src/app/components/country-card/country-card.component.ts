import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ObservedCountry } from '../../services/country.service';

@Component({
  selector: 'app-country-card',
  standalone: false,
  templateUrl: './country-card.component.html',
  styleUrl: './country-card.component.css'
})
export class CountryCardComponent {
  @Input() country: ObservedCountry | null = null;
  @Input() skeleton = false;
  @Output() delete = new EventEmitter<ObservedCountry>();
  @Output() edit = new EventEmitter<ObservedCountry>();
  @Output() explore = new EventEmitter<ObservedCountry>();
}
