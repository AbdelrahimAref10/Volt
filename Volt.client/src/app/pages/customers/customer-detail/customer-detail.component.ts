import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminCustomerClient, CustomerDto, CustomerState } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-customer-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './customer-detail.component.html',
  styleUrl: './customer-detail.component.css'
})
export class CustomerDetailComponent implements OnInit {
  customer: CustomerDto | null = null;
  customerId: number = 0;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  actionLoading: string = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private customerClient: AdminCustomerClient
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.customerId = +params['id'];
      if (this.customerId) {
        this.loadCustomer();
      }
    });
  }

  loadCustomer(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.customerClient.getById(this.customerId).subscribe({
      next: (customer: CustomerDto) => {
        this.customer = customer;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load customer details.';
        this.isLoading = false;
        console.error('Error loading customer:', error);
      }
    });
  }

  onActivate(): void {
    if (!confirm('Are you sure you want to activate this customer?')) return;
    
    this.actionLoading = 'activate';
    this.customerClient.activate(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Customer activated successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to activate customer');
        this.actionLoading = '';
      }
    });
  }

  onDeactivate(): void {
    if (!confirm('Are you sure you want to deactivate this customer?')) return;
    
    this.actionLoading = 'deactivate';
    this.customerClient.deactivate(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Customer deactivated successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to deactivate customer');
        this.actionLoading = '';
      }
    });
  }

  onBlock(): void {
    if (!confirm('Are you sure you want to block this customer?')) return;
    
    this.actionLoading = 'block';
    this.customerClient.block(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Customer blocked successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to block customer');
        this.actionLoading = '';
      }
    });
  }

  onUnblock(): void {
    if (!confirm('Are you sure you want to unblock this customer?')) return;
    
    this.actionLoading = 'unblock';
    this.customerClient.unblock(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Customer unblocked successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to unblock customer');
        this.actionLoading = '';
      }
    });
  }

  onBlockCash(): void {
    if (!confirm('Are you sure you want to block this customer from cash payment?')) return;
    
    this.actionLoading = 'blockCash';
    this.customerClient.blockCash(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Cash payment blocked successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to block cash payment');
        this.actionLoading = '';
      }
    });
  }

  onUnblockCash(): void {
    if (!confirm('Are you sure you want to unblock this customer from cash payment?')) return;
    
    this.actionLoading = 'unblockCash';
    this.customerClient.unblockCash(this.customerId).subscribe({
      next: () => {
        this.showSuccessMessage('Cash payment unblocked successfully');
        this.loadCustomer();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to unblock cash payment');
        this.actionLoading = '';
      }
    });
  }

  onBack(): void {
    this.router.navigate(['/main/customers']);
  }

  getStateLabel(state: CustomerState): string {
    switch (state) {
      case CustomerState.Active:
        return 'Active';
      case CustomerState.InActive:
        return 'InActive';
      case CustomerState.Blocked:
        return 'Blocked';
      default:
        return 'Unknown';
    }
  }

  getStateClass(state: CustomerState): string {
    switch (state) {
      case CustomerState.Active:
        return 'customer-detail__status--active';
      case CustomerState.InActive:
        return 'customer-detail__status--inactive';
      case CustomerState.Blocked:
        return 'customer-detail__status--blocked';
      default:
        return '';
    }
  }

  showSuccessMessage(message: string): void {
    this.successMessage = message;
    this.errorMessage = '';
    setTimeout(() => {
      this.successMessage = '';
    }, 5000);
  }

  showErrorMessage(message: string): void {
    this.errorMessage = message;
    this.successMessage = '';
    setTimeout(() => {
      this.errorMessage = '';
    }, 5000);
  }

  isActionLoading(action: string): boolean {
    return this.actionLoading === action;
  }

  canActivate(): boolean {
    return this.customer?.state === CustomerState.InActive;
  }

  canDeactivate(): boolean {
    return this.customer?.state === CustomerState.Active;
  }

  canBlock(): boolean {
    return this.customer?.state === CustomerState.Active;
  }

  canUnblock(): boolean {
    return this.customer?.state === CustomerState.Blocked;
  }

  get CustomerState() {
    return CustomerState;
  }
}

