import { Component, Input } from '@angular/core';

export interface SectionRow {
  label: string;
  value: string | null | undefined;
  isArray?: boolean;
  arrayValue?: string[];
  showMore?: boolean;
  onShowMore?: () => void;
}

@Component({
  selector: 'app-section-card',
  standalone: false,
  templateUrl: './section-card.component.html',
  styleUrl: './section-card.component.css'
})

export class SectionCardComponent {
  @Input() title = '';
  @Input() rows: SectionRow[] = [];

  get visibleRows(): SectionRow[] {
    return this.rows.filter(r => {
      if(r.isArray) {
        return r.arrayValue && r.arrayValue.length > 0;
      }

      return r.value !== null && r.value !== undefined && r.value !== '';
    });
  }

  get hasData(): boolean {
    return this.visibleRows.length > 0;
  }
}
