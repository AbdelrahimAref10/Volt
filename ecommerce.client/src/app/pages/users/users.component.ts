import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="users">
      <div class="users__header">
        <h1 class="users__title">Users</h1>
      </div>
      <div class="users__content">
        <p class="users__placeholder">Users component - Coming soon</p>
      </div>
    </div>
  `,
  styles: [`
    .users {
      @apply p-6;
    }
    .users__header {
      @apply mb-6;
    }
    .users__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .users__placeholder {
      @apply text-neutral-600;
    }
  `]
})
export class UsersComponent {
}


