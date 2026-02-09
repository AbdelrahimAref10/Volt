import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminReportsClient, RevenueReportDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-revenue-by-period-report',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './revenue-by-period-report.component.html',
  styleUrls: ['../report-page-shared.css', './revenue-by-period-report.component.css']
})
export class RevenueByPeriodReportComponent implements OnInit {
  data: RevenueReportDto[] = [];
  isLoading = false;
  errorMessage = '';
  period = 'month';
  numberOfPeriods = 6;

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
    this.reportsClient.getRevenueByPeriodReport(this.period, this.numberOfPeriods).subscribe({
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

  formatNumber(n: number): string {
    return n != null ? new Intl.NumberFormat('en-EG').format(n) : '—';
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
