import { Component, Input } from '@angular/core';
import { WeatherData } from '../../services/country.service';

@Component({
  selector: 'app-weather-card',
  standalone: false,
  templateUrl: './weather-card.component.html',
  styleUrl: './weather-card.component.css'
})
export class WeatherCardComponent {
  @Input() weather: WeatherData | null = null;
  @Input() capital: string = '';
  @Input() commonName: string = '';

  get locationLabel(): string {
    if(!this.weather) {
      return '';
    }

    return this.weather.location === 'capital' ? this.capital : this.commonName;
  }

  get isDayOrNight(): string {
    if(!this.weather) {
      return '';
    }

    return this.weather.isDay ? 'Day' : 'Night';
  }

  get windDirectionLabel(): string {
    if(!this.weather) {
      return '';
    }

    const deg = this.weather.windDirection;
    const dirs = ['N', 'NE', 'E', 'SE', 'S', 'SW', 'W', 'NW'];

    return dirs[Math.round(deg / 45) % 8];
  }
}
