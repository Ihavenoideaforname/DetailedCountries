import { Component, OnInit } from '@angular/core';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: false,
  styleUrl: './app.component.css'
})
export class AppComponent implements OnInit {
  constructor(private titleService: Title) { }

  title = 'Detailed Countries';

  ngOnInit() {
    this.titleService.setTitle(this.title);
  }
}
