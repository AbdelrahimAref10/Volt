import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminReportsClient, RevenueReportDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-revenue-report',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './revenue-report.component.html',
  styleUrls: ['../report-page-shared.css', './revenue-report.component.css']
})
export class RevenueReportComponent implements OnInit {
  data: RevenueReportDto | null = null;
  isLoading = false;
  errorMessage = '';
  period = 'month';

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
    this.reportsClient.getRevenueReport(this.period).subscribe({
      next: (d) => {
        this.data = d;
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
