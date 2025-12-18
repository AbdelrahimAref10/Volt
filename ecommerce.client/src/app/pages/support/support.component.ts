import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-support',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="support">
      <div class="support__header">
        <h1 class="support__title">Support</h1>
      </div>
      <div class="support__content">
        <p class="support__placeholder">Support component - Coming soon</p>
      </div>
    </div>
  `,
  styles: [`
    .support {
      @apply p-6;
    }
    .support__header {
      @apply mb-6;
    }
    .support__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .support__placeholder {
      @apply text-neutral-600;
    }
  `]
})
export class SupportComponent {
}


