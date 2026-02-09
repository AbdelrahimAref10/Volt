import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

export interface ReportItem {
  route: string;
  title: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.css'
})
export class ReportsComponent {
  reportItems: ReportItem[] = [
    { route: 'orders-by-state', title: 'Orders by State', description: 'Order counts grouped by status (Pending, Confirmed, On Way, etc.)', icon: 'chart' },
    { route: 'orders-by-date-range', title: 'Orders by Date Range', description: 'Filter orders between two dates', icon: 'calendar' },
    { route: 'revenue', title: 'Revenue', description: 'Revenue summary by period (month, quarter, year)', icon: 'currency' },
    { route: 'revenue-by-period', title: 'Revenue by Period', description: 'Revenue breakdown across multiple periods', icon: 'currency' },
    { route: 'cancellations', title: 'Cancellations', description: 'Cancelled orders and fee totals (paid vs unpaid)', icon: 'x-circle' },
    { route: 'cancellation-fees', title: 'Cancellation Fees', description: 'List of all order cancellation fee entries', icon: 'x-circle' },
    { route: 'vehicle-utilization', title: 'Vehicle Utilization', description: 'How vehicles are used across orders', icon: 'truck' },
    { route: 'customer-order-history', title: 'Customer Order History', description: 'Orders for a specific customer (use customer ID)', icon: 'user' },
    { route: 'treasury-balance', title: 'Treasury Balance', description: 'Company treasury balance and movements', icon: 'wallet' },
    { route: 'wallet', title: 'Wallet Report', description: 'Customer wallet entries with filters (customer, state)', icon: 'wallet' },
  ];
}
