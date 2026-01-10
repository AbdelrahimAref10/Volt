import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="reports">
      <div class="reports__header">
        <h1 class="reports__title">Reports</h1>
      </div>
      <div class="reports__content">
        <p class="reports__placeholder">Reports component - Coming soon</p>
      </div>
    </div>
  `,
  styles: [`
    .reports {
      @apply p-6;
    }
    .reports__header {
      @apply mb-6;
    }
    .reports__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .reports__placeholder {
      @apply text-neutral-600;
    }
  `]
})
export class ReportsComponent {
}


