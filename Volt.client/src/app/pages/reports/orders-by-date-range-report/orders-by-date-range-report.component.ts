import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminReportsClient, OrderDto, OrderState } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-orders-by-date-range-report',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './orders-by-date-range-report.component.html',
  styleUrls: ['../report-page-shared.css', './orders-by-date-range-report.component.css']
})
export class OrdersByDateRangeReportComponent implements OnInit {
  data: OrderDto[] = [];
  isLoading = false;
  errorMessage = '';
  fromDate = '';
  toDate = '';

  constructor(
    private router: Router,
    private reportsClient: AdminReportsClient
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.isLoading = true;
    this.errorMessage = '';
    const from = this.fromDate ? new Date(this.fromDate) : undefined;
    const to = this.toDate ? new Date(this.toDate) : undefined;
    this.reportsClient.getOrdersByDateRangeReport(from, to).subscribe({
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
