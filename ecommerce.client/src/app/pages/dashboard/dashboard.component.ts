import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="dashboard">
      <h1 class="dashboard__title">Dashboard</h1>
      <p class="dashboard__subtitle">Welcome to ScooterRent Admin Dashboard</p>
    </div>
  `,
  styles: [`
    .dashboard {
      @apply p-6;
    }
    .dashboard__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .dashboard__subtitle {
      @apply text-neutral-600;
    }
  `]
})
export class DashboardComponent {
}


