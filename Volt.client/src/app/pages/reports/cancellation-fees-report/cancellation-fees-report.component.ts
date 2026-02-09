import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { AdminReportsClient, CustomerWalletDto, CustomerWalletState } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-cancellation-fees-report',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './cancellation-fees-report.component.html',
  styleUrls: ['../report-page-shared.css', './cancellation-fees-report.component.css']
})
export class CancellationFeesReportComponent implements OnInit {
  data: CustomerWalletDto[] = [];
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
    this.reportsClient.getCancellationFeesReport().subscribe({
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

  getStateLabel(s: CustomerWalletState): string {
    return s === CustomerWalletState.Pending ? 'Pending' : 'Paid';
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
