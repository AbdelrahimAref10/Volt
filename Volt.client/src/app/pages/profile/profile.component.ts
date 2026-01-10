import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="profile">
      <div class="profile__header">
        <h1 class="profile__title">Profile</h1>
      </div>
      <div class="profile__content">
        <p class="profile__placeholder">Profile component - Coming soon</p>
      </div>
    </div>
  `,
  styles: [`
    .profile {
      @apply p-6;
    }
    .profile__header {
      @apply mb-6;
    }
    .profile__title {
      @apply text-3xl font-bold text-neutral-900 mb-2;
    }
    .profile__placeholder {
      @apply text-neutral-600;
    }
  `]
})
export class ProfileComponent {
}


