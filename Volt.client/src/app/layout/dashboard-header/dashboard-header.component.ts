import { Component, EventEmitter, Output, HostListener, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Subscription } from 'rxjs';
import { AuthService } from '../../core/services/auth.service';
import { AdminNotificationService, AdminNotification } from '../../core/services/admin-notification.service';
import { NotificationDropdownComponent, Notification } from './notification-dropdown/notification-dropdown.component';

@Component({
  selector: 'app-dashboard-header',
  standalone: true,
  imports: [CommonModule, RouterModule, NotificationDropdownComponent],
  templateUrl: './dashboard-header.component.html',
  styleUrl: './dashboard-header.component.css'
})
export class DashboardHeaderComponent implements OnInit, OnDestroy {
  @Output() toggleSidebar = new EventEmitter<void>();
  
  isUserMenuOpen = false;
  isNotificationOpen = false;
  userData: any;
  
  notifications: Notification[] = [];
  unreadCount: number = 0;
  private subscriptions = new Subscription();

  constructor(
    private authService: AuthService,
    private notificationService: AdminNotificationService
  ) {
    this.userData = this.authService.getUserData();
  }

  ngOnInit(): void {
    // Load initial notifications
    this.loadNotifications();
    this.loadUnreadCount();

    // Subscribe to notifications
    this.subscriptions.add(
      this.notificationService.notifications$.subscribe(notifications => {
        this.notifications = notifications.map(n => this.mapToNotification(n));
      })
    );

    // Subscribe to unread count
    this.subscriptions.add(
      this.notificationService.unreadCount$.subscribe(count => {
        this.unreadCount = count;
      })
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  loadNotifications(): void {
    this.notificationService.loadNotifications(false, 0, 50).subscribe();
  }

  loadUnreadCount(): void {
    this.notificationService.updateUnreadCount();
  }

  mapToNotification(adminNotification: AdminNotification): Notification {
    return {
      id: adminNotification.adminNotificationId.toString(),
      type: this.getNotificationType(adminNotification.notificationType),
      title: adminNotification.title,
      message: adminNotification.message,
      timestamp: adminNotification.createdDate,
      isRead: adminNotification.isRead,
      actionUrl: adminNotification.orderId ? `/main/orders/${adminNotification.orderId}` : undefined
    };
  }

  getNotificationType(notificationType: number): 'system' | 'user' | 'order' | 'warning' | 'info' {
    // Map backend NotificationType enum to frontend types
    switch (notificationType) {
      case 1: // OrderCreated
      case 2: // OrderConfirmed
      case 3: // OrderOnWay
      case 4: // OrderCustomerReceived
      case 5: // OrderCompleted
      case 6: // OrderCancelled
        return 'order';
      default:
        return 'info';
    }
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
    const id = parseInt(notificationId, 10);
    this.notificationService.markAsRead(id).subscribe({
      next: () => {
        console.log('Notification marked as read');
      },
      error: (error) => {
        console.error('Error marking notification as read:', error);
      }
    });
  }

  onMarkAllAsRead(): void {
    // Mark all unread notifications as read
    const unreadNotifications = this.notifications.filter(n => !n.isRead);
    unreadNotifications.forEach(notification => {
      this.onMarkAsRead(notification.id);
    });
  }

  onNotificationClick(notification: Notification): void {
    if (notification.actionUrl) {
      // Navigation will be handled by routerLink in the template
    }
    // Mark as read when clicked
    if (!notification.isRead) {
      this.onMarkAsRead(notification.id);
    }
    this.isNotificationOpen = false;
  }

  onLogout(): void {
    this.authService.logout();
  }
}


