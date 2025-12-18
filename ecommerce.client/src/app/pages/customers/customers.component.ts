import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminCustomerClient, CustomerDto, PagedResultOfCustomerDto, UpdateCustomerCommand, CustomerState } from '../../core/services/clientAPI';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.css'
})
export class CustomersComponent implements OnInit {
  customers: CustomerDto[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  selectedState: CustomerState | null = null;
  isLoading = false;
  errorMessage = '';
  
  showModal = false;
  isEditMode = false;
  customerForm: FormGroup;
  selectedCustomerId: number | null = null;

  customerStates = [
    { value: null, label: 'All States' },
    { value: CustomerState.InActive, label: 'InActive' },
    { value: CustomerState.Active, label: 'Active' },
    { value: CustomerState.Blocked, label: 'Blocked' }
  ];

  constructor(
    private customerClient: AdminCustomerClient,
    private fb: FormBuilder
  ) {
    this.customerForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(2)]],
      nationalNumber: ['', [Validators.required]],
      gender: ['', [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.loadCustomers();
  }

  loadCustomers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.customerClient.getAll(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedState !== null ? this.selectedState : undefined
    ).subscribe({
      next: (result: PagedResultOfCustomerDto) => {
        this.customers = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load customers. Please try again.';
        this.isLoading = false;
        console.error('Error loading customers:', error);
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCustomers();
  }

  onStateFilter(state: CustomerState | null): void {
    this.selectedState = state;
    this.currentPage = 1;
    this.loadCustomers();
  }

  onEdit(customer: CustomerDto): void {
    this.isEditMode = true;
    this.selectedCustomerId = customer.customerId;
    this.customerForm.patchValue({
      userName: customer.userName,
      nationalNumber: customer.nationalNumber,
      gender: customer.gender
    });
    this.showModal = true;
  }

  onBlock(customerId: number): void {
    if (confirm('Are you sure you want to block this customer?')) {
      this.customerClient.block(customerId).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error:any) => {
          alert('Failed to block customer. Please try again.');
          console.error('Error blocking customer:', error);
        }
      });
    }
  }

  onUnblock(customerId: number): void {
    if (confirm('Are you sure you want to unblock this customer?')) {
      this.customerClient.unblock(customerId).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error: any) => {
          alert('Failed to unblock customer. Please try again.');
          console.error('Error unblocking customer:', error);
        }
      });
    }
  }

  onDelete(customerId: number): void {
    if (confirm('Are you sure you want to delete this customer? This will delete all customer data permanently.')) {
      this.customerClient.delete(customerId).subscribe({
        next: () => {
          this.loadCustomers();
        },
        error: (error: any) => {
          alert('Failed to delete customer. Please try again.');
          console.error('Error deleting customer:', error);
        }
      });
    }
  }

  onSubmit(): void {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    if (!this.selectedCustomerId) {
      return;
    }

    const formValue = this.customerForm.value;
    const command = new UpdateCustomerCommand();
    command.customerId = this.selectedCustomerId;
    command.userName = formValue.userName;
    command.nationalNumber = formValue.nationalNumber;
    command.gender = formValue.gender;

    this.customerClient.update(this.selectedCustomerId, command).subscribe({
      next: () => {
        this.showModal = false;
        this.loadCustomers();
      },
      error: (error: any) => {
        alert('Failed to update customer. Please try again.');
        console.error('Error updating customer:', error);
      }
    });
  }

  onCloseModal(): void {
    this.showModal = false;
    this.customerForm.reset();
    this.selectedCustomerId = null;
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCustomers();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPages - 1);
    
    if (endPage - startPage < maxPages - 1) {
      startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
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
        return 'customers__state--active';
      case CustomerState.InActive:
        return 'customers__state--inactive';
      case CustomerState.Blocked:
        return 'customers__state--blocked';
      default:
        return '';
    }
  }
}

