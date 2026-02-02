import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  AdminReportsClient,
  WalletReportEntryDto,
  CustomerWalletState,
} from '../../../core/services/clientAPI';

@Component({
  selector: 'app-wallet-report',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './wallet-report.component.html',
  styleUrls: ['../report-page-shared.css', './wallet-report.component.css']
})
export class WalletReportComponent implements OnInit {
  data: WalletReportEntryDto[] = [];
  isLoading = false;
  errorMessage = '';
  customerId = '';
  customerSearch = '';
  walletState: CustomerWalletState | '' = '';

  CustomerWalletState = CustomerWalletState;

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
    const cId = this.customerId ? +this.customerId : undefined;
    const wState = this.walletState === '' ? undefined : this.walletState;
    this.reportsClient.getWalletReport(cId, this.customerSearch || undefined, wState).subscribe({
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

  getTypeLabel(type: number): string {
    const map: Record<number, string> = { 0: 'Penalty', 1: 'Bonus', 2: 'Order Cancellation Fees' };
    return map[type] ?? 'Unknown';
  }

  onBack(): void {
    this.router.navigate(['/main/reports']);
  }
}
