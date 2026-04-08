import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { CountryListItem } from '../../services/country.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private titleService: Title) { }

  title = 'Detailed Countries - Home';
  showModal = false;

  ngOnInit() {
    this.titleService.setTitle(this.title);
  }

  onCountryAdded(country: CountryListItem) {
    console.log('Added: ', country);
  }
}
