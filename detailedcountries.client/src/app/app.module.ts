import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { HomeComponent } from './pages/home/home.component';
import { NavbarComponent } from './components/navbar/navbar.component';
import { FooterComponent } from './components/footer/footer.component';
import { AddCountryModalComponent } from './components/add-country-modal/add-country-modal.component';
import { CountriesComponent } from './pages/countries/countries.component';
import { CountryCardComponent } from './components/country-card/country-card.component';
import { EditCountryModalComponent } from './components/edit-country-modal/edit-country-modal.component';
import { RemoveCountryModalComponent } from './components/remove-country-modal/remove-country-modal.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent,
    NavbarComponent,
    FooterComponent,
    AddCountryModalComponent,
    CountriesComponent,
    CountryCardComponent,
    EditCountryModalComponent,
    RemoveCountryModalComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    CommonModule,
    FormsModule
  ],
  providers: [
    provideHttpClient(withInterceptorsFromDi())
  ],
  bootstrap: [AppComponent]
})
export class AppModule { }
