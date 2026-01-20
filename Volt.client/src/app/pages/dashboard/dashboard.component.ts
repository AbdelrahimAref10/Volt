import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminDashboardClient, AdminDashboardAnalyticsDto } from '../../core/services/clientAPI';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  analytics: AdminDashboardAnalyticsDto | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private dashboardClient: AdminDashboardClient,
    private toastr: ToastrService
  ) {}

  ngOnInit(): void {
    this.loadDashboardData();
  }

  loadDashboardData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.dashboardClient.getDashboardAnalytics(10, 5, 6).subscribe({
      next: (data: AdminDashboardAnalyticsDto) => {
        this.analytics = data;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load dashboard analytics. Please try again.';
        this.isLoading = false;
        this.toastr.error('Failed to load dashboard data', 'Error');
        console.error('Error loading dashboard analytics:', error);
      }
    });
  }

  refresh(): void {
    this.loadDashboardData();
  }

  formatCurrency(amount: number | null | undefined): string {
    if (amount == null) return '$0.00';
    return new Intl.NumberFormat('en-US', {
      style: 'currency',
      currency: 'USD',
      minimumFractionDigits: 2
    }).format(amount);
  }

  formatNumber(value: number | null | undefined): string {
    if (value == null) return '0';
    return new Intl.NumberFormat('en-US').format(value);
  }

  formatDate(date: Date | string | null | undefined): string {
    if (!date) return 'N/A';
    const d = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(d.getTime())) return 'Invalid Date';
    return new Intl.DateTimeFormat('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }).format(d);
  }

  formatPercentage(value: number | null | undefined): string {
    if (value == null) return '0.0%';
    return `${value.toFixed(1)}%`;
  }

  getOrderStateClass(state: string): string {
    const stateLower = state.toLowerCase();
    if (stateLower.includes('pending')) return 'dashboard__badge--pending';
    if (stateLower.includes('confirmed')) return 'dashboard__badge--confirmed';
    if (stateLower.includes('onway') || stateLower.includes('on way')) return 'dashboard__badge--onway';
    if (stateLower.includes('received')) return 'dashboard__badge--received';
    if (stateLower.includes('completed')) return 'dashboard__badge--completed';
    if (stateLower.includes('cancelled')) return 'dashboard__badge--cancelled';
    return 'dashboard__badge--default';
  }

  getOrderStateLabel(state: string): string {
    return state.replace(/([A-Z])/g, ' $1').trim();
  }

  getRevenueBarHeight(revenue: number | null | undefined): number {
    if (!this.analytics || !this.analytics.revenueAnalytics?.revenueByPeriod || this.analytics.revenueAnalytics.revenueByPeriod.length === 0 || !revenue) {
      return 0;
    }
    const maxRevenue = Math.max(...this.analytics.revenueAnalytics.revenueByPeriod.map(p => p.totalRevenue || 0));
    if (maxRevenue === 0) return 0;
    return (revenue / maxRevenue) * 100;
  }

  getStatePercentage(count: number | null | undefined): number {
    if (!this.analytics || !this.analytics.ordersByState || this.analytics.ordersByState.length === 0 || !count) {
      return 0;
    }
    const total = this.analytics.ordersByState.reduce((sum, s) => sum + (s.count || 0), 0);
    if (total === 0) return 0;
    return (count / total) * 100;
  }
}
