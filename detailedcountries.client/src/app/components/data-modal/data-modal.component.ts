import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-data-modal',
  standalone: false,
  templateUrl: './data-modal.component.html',
  styleUrl: './data-modal.component.css'
})
export class DataModalComponent {
  @Input() title = '';
  @Input() items: string[] = [];
  @Output() closed = new EventEmitter<void>();
}
