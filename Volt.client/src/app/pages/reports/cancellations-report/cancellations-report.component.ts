import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AdminReportsClient, CancellationReportDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-cancellations-report',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cancellations-report.component.html',
  styleUrls: ['../report-page-shared.css', './cancellations-report.component.css']
})
export class CancellationsReportComponent implements OnInit {
  data: CancellationReportDto | null = null;
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
    this.reportsClient.getCancellationReport().subscribe({
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
