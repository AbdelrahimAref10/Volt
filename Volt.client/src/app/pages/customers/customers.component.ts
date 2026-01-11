import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { AdminCustomerClient, CustomerDto, PagedResultOfCustomerDto, CustomerState, CityClient, CityDto, PagedResultOfCityDto, AdminCreateCustomerCommand } from '../../core/services/clientAPI';

@Component({
  selector: 'app-customers',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './customers.component.html',
  styleUrl: './customers.component.css'
})
export class CustomersComponent implements OnInit, OnDestroy {
  customers: CustomerDto[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  selectedState: CustomerState | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  searchFilters = {
    state: null as CustomerState | null,
    customerType: null as number | null,
    cityId: null as number | null
  };

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  showModal = false;
  customerForm: FormGroup;
  cities: CityDto[] = [];
  isLoadingCities = false;
  isSubmitting = false;

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
    private cityClient: CityClient,
    private fb: FormBuilder,
    private router: Router
  ) {
    this.customerForm = this.fb.group({
      mobileNumber: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
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
    this.loadCities();
    this.setupLiveSearch();
    // Initial load
    this.triggerSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.searchSubject.complete();
  }

  setupLiveSearch(): void {
    this.searchSubject.pipe(
      debounceTime(300), // Wait 300ms after user stops typing
      distinctUntilChanged(),
      switchMap((searchTerm) => {
        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        const state = this.searchFilters.state !== null ? this.searchFilters.state :
                      (this.selectedState !== null ? this.selectedState : undefined);
        const cityId = this.searchFilters.cityId !== null && this.searchFilters.cityId > 0 ? this.searchFilters.cityId : undefined;
        const registerAs = this.searchFilters.customerType !== null ? this.searchFilters.customerType : undefined;

        return this.customerClient.getAll(
          this.currentPage,
          this.pageSize,
          searchTerm || undefined,
          state,
          cityId,
          registerAs
        );
      }),
      takeUntil(this.destroy$)
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

  triggerSearch(): void {
    this.searchSubject.next(this.searchTerm);
  }

  loadCities(): void {
    this.isLoadingCities = true;
    this.cityClient.getAll(1, 1000, undefined, true).subscribe({
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
    this.triggerSearch();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onStateFilter(state: CustomerState | null): void {
    this.selectedState = state;
    this.currentPage = 1;
    this.triggerSearch();
  }

  onAdvancedSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onSearchTermChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onClearSearch(): void {
    this.searchFilters = {
      state: null,
      customerType: null,
      cityId: null
    };
    this.searchTerm = '';
    this.selectedState = null;
    this.currentPage = 1;
    this.loadCustomers();
  }

  hasActiveFilters(): boolean {
    return this.searchFilters.state !== null ||
           this.searchFilters.customerType !== null ||
           (this.searchFilters.cityId !== null && this.searchFilters.cityId > 0) ||
           (this.searchTerm && this.searchTerm.trim().length > 0) ||
           this.selectedState !== null;
  }

  onAddNew(): void {
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

  onView(customerId: number): void {
    this.router.navigate(['/main/customers', customerId]);
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

    this.isSubmitting = true;
    const formValue = this.customerForm.value;

    // Create new customer
    let personalImageBase64 = null;

    if (this.selectedImage) {
      personalImageBase64 = await this.convertImageToBase64(this.selectedImage);
    }

    const command = new AdminCreateCustomerCommand();
    command.mobileNumber = formValue.mobileNumber;
    command.fullName = formValue.fullName;
    command.gender = formValue.gender;
    command.cityId = formValue.cityId;
    command.fullAddress = formValue.fullAddress || null;
    command.personalImage = personalImageBase64;
    command.registerAs = formValue.registerAs;
    command.verificationBy = formValue.verificationBy;
    command.password = formValue.password;

    this.customerClient.create(command).subscribe({
      next: () => {
        this.showModal = false;
        this.showSuccessMessage('Customer created successfully');
        this.loadCustomers();
        this.isSubmitting = false;
      },
      error: (error: any) => {
        const errorMessage = error.error?.detail || error.error?.title || 'Failed to create customer. Please try again.';
        this.showErrorMessage(errorMessage);
        this.isSubmitting = false;
        console.error('Error creating customer:', error);
      }
    });
  }

  onCloseModal(): void {
    this.showModal = false;
    this.customerForm.reset();
    this.selectedImage = null;
    this.imagePreview = null;
    this.isSubmitting = false;
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.triggerSearch();
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

  getCashBlockClass(cashBlock: boolean): string {
    return cashBlock ? 'customers__cash-block--blocked' : 'customers__cash-block--allowed';
  }

  getCashBlockLabel(cashBlock: boolean): string {
    return cashBlock ? 'Blocked' : 'Allowed';
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


  get activeCustomersCount(): number {
    return this.customers.filter(c => c.state === CustomerState.Active).length;
  }

  get inactiveCustomersCount(): number {
    return this.customers.filter(c => c.state === CustomerState.InActive).length;
  }

  get blockedCustomersCount(): number {
    return this.customers.filter(c => c.state === CustomerState.Blocked).length;
  }
}
