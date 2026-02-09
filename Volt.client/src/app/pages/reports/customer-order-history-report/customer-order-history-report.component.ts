import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminReportsClient, OrderDto, OrderState } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-customer-order-history-report',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './customer-order-history-report.component.html',
  styleUrls: ['../report-page-shared.css', './customer-order-history-report.component.css']
})
export class CustomerOrderHistoryReportComponent {
  data: OrderDto[] = [];
  isLoading = false;
  errorMessage = '';
  customerId = '';

  constructor(
    private router: Router,
    private reportsClient: AdminReportsClient
  ) {}

  load(): void {
    if (!this.customerId?.trim()) {
      this.errorMessage = 'Please enter Customer ID.';
      return;
    }
    this.isLoading = true;
    this.errorMessage = '';
    const cId = +this.customerId;
    this.reportsClient.getCustomerOrderHistoryReport(cId).subscribe({
      next: (list) => {
        this.data = list;
        this.isLoading = false;
      },
      error: (err) => {
        this.errorMessage = err.error?.errorMessage || err.message || 'Failed to load report';
        this.isLoading = false;
      }
    });
  }

  formatCurrency(n: number): string {
    return n != null ? new Intl.NumberFormat('en-EG', { minimumFractionDigits: 2, maximumFractionDigits: 2 }).format(n) + ' EGP' : '—';
  }

  getOrderStateLabel(state: OrderState): string {
    const map: Record<OrderState, string> = {
      [OrderState.Pending]: 'Pending',
      [OrderState.Confirmed]: 'Confirmed',
      [OrderState.OnWay]: 'On Way',
      [OrderState.CustomerReceived]: 'Customer Received',
      [OrderState.Completed]: 'Completed',
    };
    return map[state] ?? 'Unknown';
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
