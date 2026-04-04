import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-home',
  standalone: false,
  templateUrl: './home.component.html',
  styleUrl: './home.component.css'
})
export class HomeComponent {
  constructor(private titleService: Title) { }

  title = 'Detailed Countries';

  ngOnInit() {
    this.titleService.setTitle(this.title);
  }

  openAddModal() {
    console.log('Add button clicked');
  }
}
