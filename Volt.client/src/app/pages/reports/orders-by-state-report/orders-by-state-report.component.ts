import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AdminReportsClient, OrdersByStateReportDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-orders-by-state-report',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './orders-by-state-report.component.html',
  styleUrls: ['../report-page-shared.css', './orders-by-state-report.component.css']
})
export class OrdersByStateReportComponent implements OnInit {
  data: OrdersByStateReportDto[] = [];
  isLoading = false;
  errorMessage = '';

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
    this.reportsClient.getOrdersByStateReport().subscribe({
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

  formatNumber(n: number): string {
    return n != null ? new Intl.NumberFormat('en-EG').format(n) : '—';
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
