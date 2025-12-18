import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent {
  @Output() toggleSidebar = new EventEmitter<void>();
  
  isUserMenuOpen = false;
  userData: any;

  constructor(private authService: AuthService) {
    this.userData = this.authService.getUserData();
  }

  onToggleSidebar(): void {
    this.toggleSidebar.emit();
  }

  onToggleUserMenu(): void {
    this.isUserMenuOpen = !this.isUserMenuOpen;
  }

  onLogout(): void {
    this.authService.logout();
  }
}


