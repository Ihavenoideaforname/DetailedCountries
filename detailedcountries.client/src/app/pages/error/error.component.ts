import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Title } from '@angular/platform-browser';

@Component({
  selector: 'app-error',
  standalone: false,
  templateUrl: './error.component.html',
  styleUrl: './error.component.css'
})
export class ErrorComponent {
  status = 500;
  message = 'An unexpected server error occurred.';

  constructor(private router: Router, private titleService: Title) { }

  ngOnInit() {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state;

    if(state) {
      this.status = state['status'] ?? this.status;
      this.message = state['message'] ?? this.message;
    }

    this.titleService.setTitle('Detailed Countries - Error');
  }

  goHome() {
    this.router.navigate(['/home']);
  }

  goBack() {
    history.back();
  }
}
