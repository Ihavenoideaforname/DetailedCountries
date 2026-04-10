import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';
import { CountryListItem } from '../../services/country.service';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private router: Router, private titleService: Title) { }

  title = 'Detailed Countries - Home';
  showModal = false;

  ngOnInit() {
    this.titleService.setTitle(this.title);
  }

  onCountryAdded(country: CountryListItem) {
    console.log('Added: ', country);
    this.router.navigate(['/countries']);
  }
}
