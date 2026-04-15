import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface CountryListItem {
  Id: string;
  Code: string;
  Name: string;
  Flag: string;
  Alt: string;
}

export interface ObservedCountry extends CountryListItem {
  OfficialName: string;
}

export interface WeatherData {
  temperature: number;
  windspeed: number;
  windDirection: number;
  isDay: boolean;
  condition: string;
  location: string;
}

export interface NeighbourData {
  code: string;
  isObserved: boolean;
  listItem: CountryListItem | null;
}

export interface CountryDetails {
  id: string;
  commonName: string;
  officialName: string;
  nativeCommonName: string | null;
  nativeOfficialName: string | null;
  nativeLanguage: string | null;
  flagSvg: string | null;
  flagAlt: string | null;
  coatOfArmsSvg: string | null;
  coatOfArmsAlt: string | null;
  mapUrl: string | null;
  latitude: number | null;
  longitude: number | null;
  region: string;
  subregion: string;
  area: string;
  landlocked: boolean;
  timezones: string[];
  continents: string[];
  population: string;
  languages: string[];
  status: string;
  unMember: boolean;
  independent: boolean;
  cca2: string | null;
  cca3: string | null;
  ccn3: string | null;
  cioc: string | null;
  tld: string[];
  callingCode: string | null;
  capital: string;
  capitalLat: number | null;
  capitalLng: number | null;
  currencyCode: string | null;
  currencyName: string | null;
  currencySymbol: string | null;
  gini: string | null;
  weather: WeatherData | null;
  neighbours: NeighbourData[];
}

@Injectable({
  providedIn: 'root'
})

export class CountryService {
  private readonly apiUrl = 'https://localhost:7156/api/Country';

  constructor(private http: HttpClient) { }

  getAvailable(): Observable<CountryListItem[]> {
    return this.http.get<CountryListItem[]>(`${this.apiUrl}/available`);
  }

  getObserved(): Observable<ObservedCountry[]> {
    return this.http.get<ObservedCountry[]>(`${this.apiUrl}/observed`);
  }

  getObservedByCode(code: string): Observable<ObservedCountry> {
    return this.http.get<ObservedCountry>(`${this.apiUrl}/observed/${code}`);
  }

  getObservedDetails(code: string): Observable<CountryDetails> {
    return this.http.get<CountryDetails>(`${this.apiUrl}/details/${code}`);
  }

  observe(item: CountryListItem): Observable<CountryListItem> {
    return this.http.post<CountryListItem>(`${this.apiUrl}/observe`, item);
  }

  editObserved(code: string, updated: CountryListItem): Observable<CountryListItem> {
    return this.http.put<CountryListItem>(`${this.apiUrl}/edit/${code}`, updated);
  }

  removeObserved(code: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/remove/${code}`);
  }
}
