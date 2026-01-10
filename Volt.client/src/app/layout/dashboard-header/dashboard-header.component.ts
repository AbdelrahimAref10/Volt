import { Component, EventEmitter, Output, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { NotificationDropdownComponent, Notification } from './notification-dropdown/notification-dropdown.component';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationDropdownComponent],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent {
  @Output() toggleSidebar = new EventEmitter<void>();
  
  isUserMenuOpen = false;
  isNotificationOpen = false;
  userData: any;
  
  notifications: Notification[] = [
    {
      id: '1',
      type: 'system',
      title: 'System Update',
      message: 'New features have been added to the dashboard',
      timestamp: new Date(Date.now() - 2 * 60 * 60 * 1000), // 2 hours ago
      isRead: false
    },
    {
      id: '2',
      type: 'order',
      title: 'New Order',
      message: 'Order #12345 has been placed',
      timestamp: new Date(Date.now() - 5 * 60 * 60 * 1000), // 5 hours ago
      isRead: false
    },
    {
      id: '3',
      type: 'user',
      title: 'New Customer',
      message: 'A new customer has registered',
      timestamp: new Date(Date.now() - 1 * 24 * 60 * 60 * 1000), // 1 day ago
      isRead: true
    }
  ];

  constructor(private authService: AuthService) {
    this.userData = this.authService.getUserData();
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.dashboard-header__notification-menu') && 
        !target.closest('.notification-dropdown')) {
      this.isNotificationOpen = false;
    }
    if (!target.closest('.dashboard-header__user-menu') && 
        !target.closest('.dashboard-header__dropdown')) {
      this.isUserMenuOpen = false;
    }
  }

  onToggleSidebar(): void {
    this.toggleSidebar.emit();
  }

  onToggleUserMenu(event: Event): void {
    event.stopPropagation();
    this.isUserMenuOpen = !this.isUserMenuOpen;
    this.isNotificationOpen = false;
  }

  onToggleNotification(event: Event): void {
    event.stopPropagation();
    this.isNotificationOpen = !this.isNotificationOpen;
    this.isUserMenuOpen = false;
  }

  onMarkAsRead(notificationId: string): void {
    const notification = this.notifications.find(n => n.id === notificationId);
    if (notification) {
      notification.isRead = true;
    }
  }

  onMarkAllAsRead(): void {
    this.notifications.forEach(n => n.isRead = true);
  }

  onNotificationClick(notification: Notification): void {
    if (notification.actionUrl) {
      // Navigate to action URL if provided
    }
    this.isNotificationOpen = false;
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  onLogout(): void {
    this.authService.logout();
  }
}


