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

  observe(item: CountryListItem): Observable<CountryListItem> {
    return this.http.post<CountryListItem>(`${this.apiUrl}/observe`, item);
  }
}
