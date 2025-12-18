import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="orders">
      <div class="orders__header">
        <h1 class="orders__title">Orders</h1>
      </div>
      <div class="orders__content">
        <p class="orders__placeholder">Orders component - Coming soon</p>
      </div>
    </div>
  `,
  styles: [`
    .orders {
      @apply p-6;
    }
    .orders__header {
      @apply mb-6;
    }
    .orders__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .orders__placeholder {
      @apply text-neutral-600;
    }
  `]
})
export class OrdersComponent {
}


