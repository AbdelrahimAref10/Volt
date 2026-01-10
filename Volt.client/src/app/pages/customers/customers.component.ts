import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AdminCustomerClient, CustomerDto, PagedResultOfCustomerDto, UpdateCustomerCommand, CustomerState, CityDto, PagedResultOfCityDto } from '../../core/services/clientAPI';

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
  cities: CityDto[] = [];
  isLoadingCities = false;

  customerStates = [
    { value: null, label: 'All States' },
    { value: CustomerState.InActive, label: 'InActive' },
    { value: CustomerState.Active, label: 'Active' },
    { value: CustomerState.Blocked, label: 'Blocked' }
  ];

  genders = [
    { value: 'Male', label: 'Male' },
    { value: 'Female', label: 'Female' }
  ];

  registerAsOptions = [
    { value: 0, label: 'Individual' },
    { value: 1, label: 'Institution' }
  ];

  verificationByOptions = [
    { value: 0, label: 'Phone' },
    { value: 1, label: 'Email' }
  ];

  selectedImage: File | null = null;
  imagePreview: string | null = null;

  constructor(
    private customerClient: AdminCustomerClient,
    private http: HttpClient,
    private fb: FormBuilder
  ) {
    this.customerForm = this.fb.group({
      mobileNumber: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
      userName: ['', [Validators.required, Validators.minLength(2)]],
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      gender: ['', [Validators.required]],
      cityId: [0, [Validators.required, Validators.min(1)]],
      fullAddress: [''],
      personalImage: [''],
      registerAs: [0, [Validators.required]],
      verificationBy: [0, [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadCities();
  }

  loadCities(): void {
    this.isLoadingCities = true;
    const url = `/api/City?PageNumber=1&PageSize=1000&IsActive=true`;
    this.http.get<PagedResultOfCityDto>(url).subscribe({
      next: (result: PagedResultOfCityDto) => {
        this.cities = result.items || [];
        this.isLoadingCities = false;
      },
      error: (error: any) => {
        console.error('Error loading cities:', error);
        this.isLoadingCities = false;
      }
    });
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

  onAddNew(): void {
    this.isEditMode = false;
    this.selectedCustomerId = null;
    this.customerForm.reset();
    this.selectedImage = null;
    this.imagePreview = null;
    this.customerForm.patchValue({
      cityId: 0,
      gender: '',
      registerAs: 0,
      verificationBy: 0
    });
    this.showModal = true;
  }

  onEdit(customer: CustomerDto): void {
    this.isEditMode = true;
    this.selectedCustomerId = customer.customerId;
    this.selectedImage = null;
    this.imagePreview = customer.personalImage || null;
    this.customerForm.patchValue({
      userName: customer.userName,
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

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedImage = file;
      
      // Create preview
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
      };
      reader.readAsDataURL(file);
    }
  }

  convertImageToBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => resolve(reader.result as string);
      reader.onerror = error => reject(error);
    });
  }

  async onSubmit(): Promise<void> {
    if (this.customerForm.invalid) {
      this.customerForm.markAllAsTouched();
      return;
    }

    const formValue = this.customerForm.value;

    if (this.isEditMode && this.selectedCustomerId) {
      // Update existing customer
      const command = new UpdateCustomerCommand();
      command.customerId = this.selectedCustomerId;
      command.userName = formValue.userName;
      command.gender = formValue.gender;
      
      // Handle image upload
      if (this.selectedImage) {
        command.personalImage = await this.convertImageToBase64(this.selectedImage);
      } else if (this.imagePreview) {
        command.personalImage = this.imagePreview;
      }

      this.customerClient.update(this.selectedCustomerId, command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCustomers();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to update customer. Please try again.';
          alert(errorMessage);
          console.error('Error updating customer:', error);
        }
      });
    } else {
      // Create new customer
      const url = `/api/AdminCustomer`;
      let personalImageBase64 = null;
      
      if (this.selectedImage) {
        personalImageBase64 = await this.convertImageToBase64(this.selectedImage);
      }

      const command = {
        mobileNumber: formValue.mobileNumber,
        userName: formValue.userName,
        fullName: formValue.fullName,
        gender: formValue.gender,
        cityId: formValue.cityId,
        fullAddress: formValue.fullAddress || null,
        personalImage: personalImageBase64,
        registerAs: formValue.registerAs,
        verificationBy: formValue.verificationBy,
        password: formValue.password
      };

      this.http.post<number>(url, command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCustomers();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to create customer. Please try again.';
          alert(errorMessage);
          console.error('Error creating customer:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.customerForm.reset();
    this.selectedCustomerId = null;
    this.isEditMode = false;
    this.selectedImage = null;
    this.imagePreview = null;
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

