import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, RouterOutlet } from '@angular/router';
import { DashboardHeaderComponent } from '../dashboard-header/dashboard-header.component';
import { DashboardSidebarComponent } from '../dashboard-sidebar/dashboard-sidebar.component';
import { AuthService } from '../../core/services/auth.service';
import { SignalRService } from '../../core/services/signalr.service';

@Component({
  selector: 'app-dashboard-layout',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    RouterOutlet,
    DashboardHeaderComponent,
    DashboardSidebarComponent
  ],
  templateUrl: './dashboard-layout.component.html',
  styleUrl: './dashboard-layout.component.css'
})
export class DashboardLayoutComponent implements OnInit {
  isSidebarOpen = true;

  constructor(
    private authService: AuthService,
    private signalRService: SignalRService
  ) {}

  ngOnInit(): void {
    // Start SignalR connection if user is already authenticated (e.g., after page refresh)
    if (this.authService.isAuthenticated()) {
      const token = this.authService.getToken();
      if (token) {
        console.log('User already authenticated, starting SignalR connection...');
        this.signalRService.StartNotificationConnection(token);
      }
    }
  }

  toggleSidebar(): void {
    this.isSidebarOpen = !this.isSidebarOpen;
  }
}


