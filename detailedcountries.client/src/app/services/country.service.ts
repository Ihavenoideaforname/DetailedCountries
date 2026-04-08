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

  observe(item: CountryListItem): Observable<ObservedCountry> {
    return this.http.post<ObservedCountry>(`${this.apiUrl}/observe`, item);
  }
}
