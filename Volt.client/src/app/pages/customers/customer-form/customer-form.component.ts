import { Component, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { AdminCustomerClient, CustomerDto, AdminCreateCustomerCommand, CityClient, CityDto, PagedResultOfCityDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-customer-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.css'
})
export class CustomerFormComponent implements OnInit {
  @ViewChild('personalImageInput', { static: false }) personalImageInputRef?: ElementRef<HTMLInputElement>;
  @ViewChild('commercialImageInput', { static: false }) commercialImageInputRef?: ElementRef<HTMLInputElement>;

  customerForm: FormGroup;
  isEditMode = false;
  customerId: number | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  cities: CityDto[] = [];
  isLoadingCities = false;

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

  personalImagePreview: string | null = null;
  selectedPersonalImage: File | null = null;
  commercialImagePreview: string | null = null;
  selectedCommercialImage: File | null = null;
  showCommercialImageError = false;

  constructor(
    private customerClient: AdminCustomerClient,
    private cityClient: CityClient,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.customerForm = this.fb.group({
      mobileNumber: ['', [Validators.required, Validators.pattern(/^[0-9]+$/)]],
      fullName: ['', [Validators.required, Validators.minLength(2)]],
      gender: ['', [Validators.required]],
      cityId: [0, [Validators.required, Validators.min(1)]],
      email: [''],
      personalImage: [''],
      commercialRegisterImage: [''],
      registerAs: [0, [Validators.required]],
      verificationBy: [0, [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    this.loadCities();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id && id !== 'new') {
        this.customerId = +id;
        this.isEditMode = true;
        this.loadCustomer();
      } else {
        this.isEditMode = false;
        this.customerId = null;
      }
    });

    // Watch verificationBy changes to validate email
    this.customerForm.get('verificationBy')?.valueChanges.subscribe(verificationBy => {
      const emailControl = this.customerForm.get('email');
      if (verificationBy === 1) {
        emailControl?.setValidators([Validators.required, Validators.email]);
      } else {
        emailControl?.setValidators([Validators.email]);
      }
      emailControl?.updateValueAndValidity();
    });

    // Watch registerAs changes to handle commercialRegisterImage
    this.customerForm.get('registerAs')?.valueChanges.subscribe(registerAs => {
      if (registerAs === 0) {
        // Individual - clear commercial image if set
        if (this.selectedCommercialImage) {
          this.selectedCommercialImage = null;
          this.commercialImagePreview = null;
        }
      }
    });
  }

  loadCities(): void {
    this.isLoadingCities = true;
    this.cityClient.getAll(1, 1000, undefined, true).subscribe({
      next: (result: PagedResultOfCityDto) => {
        this.cities = result.items || [];
        this.isLoadingCities = false;
      },
      error: (error) => {
        console.error('Error loading cities:', error);
        this.isLoadingCities = false;
      }
    });
  }

  loadCustomer(): void {
    if (!this.customerId) return;

    this.isLoading = true;
    this.customerClient.getById(this.customerId).subscribe({
      next: (customer: CustomerDto) => {
        this.customerForm.patchValue({
          mobileNumber: customer.mobileNumber,
          fullName: customer.fullName,
          gender: customer.gender,
          cityId: customer.cityId,
          email: customer.email || '',
          registerAs: customer.registerAs || 0,
          verificationBy: customer.verificationBy || 0
        });

        if (customer.personalImage) {
          this.personalImagePreview = customer.personalImage;
        }

        if (customer.commercialRegisterImage) {
          this.commercialImagePreview = customer.commercialRegisterImage;
        }

        // Password is not required in edit mode
        this.customerForm.get('password')?.clearValidators();
        this.customerForm.get('password')?.updateValueAndValidity();

        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load customer. Please try again.';
        this.isLoading = false;
        console.error('Error loading customer:', error);
      }
    });
  }

  onPersonalImageSelect(): void {
    const input = this.personalImageInputRef?.nativeElement;
    if (input && input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedPersonalImage = file;

      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target && e.target.result) {
          this.personalImagePreview = e.target.result as string;
        }
      };
      reader.readAsDataURL(file);
    }
  }

  onCommercialImageSelect(): void {
    const input = this.commercialImageInputRef?.nativeElement;
    if (input && input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedCommercialImage = file;
      this.showCommercialImageError = false;

      const reader = new FileReader();
      reader.onload = (e: ProgressEvent<FileReader>) => {
        if (e.target && e.target.result) {
          this.commercialImagePreview = e.target.result as string;
        }
      };
      reader.readAsDataURL(file);
    }
  }

  removePersonalImage(): void {
    this.personalImagePreview = null;
    this.selectedPersonalImage = null;
    this.customerForm.patchValue({ personalImage: null });
  }

  removeCommercialImage(): void {
    this.commercialImagePreview = null;
    this.selectedCommercialImage = null;
    this.customerForm.patchValue({ commercialRegisterImage: null });
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

    this.isSaving = true;
    this.errorMessage = '';
    const formValue = this.customerForm.value;

    // Validate email if verification by email
    if (formValue.verificationBy === 1 && !formValue.email) {
      this.errorMessage = 'Email is required when verification is by email';
      this.isSaving = false;
      return;
    }

    // Validate commercial register image if institution
    if (formValue.registerAs === 1 && !this.selectedCommercialImage && !this.commercialImagePreview) {
      this.showCommercialImageError = true;
      this.errorMessage = 'Commercial Register Image is required when registering as an Institution';
      this.isSaving = false;
      return;
    }
    this.showCommercialImageError = false;

    // Create customer
    let personalImageBase64 = null;
    let commercialRegisterImageBase64 = null;

    // Only send base64 if a new image is selected
    if (this.selectedPersonalImage) {
      personalImageBase64 = await this.convertImageToBase64(this.selectedPersonalImage);
    }

    // Only set commercial register image if RegisterAs is Institution (1)
    if (formValue.registerAs === 1) {
      if (this.selectedCommercialImage) {
        commercialRegisterImageBase64 = await this.convertImageToBase64(this.selectedCommercialImage);
      }
    } else {
      commercialRegisterImageBase64 = null;
    }

    const command = new AdminCreateCustomerCommand();
    command.mobileNumber = formValue.mobileNumber;
    command.fullName = formValue.fullName;
    command.gender = formValue.gender;
    command.cityId = formValue.cityId;
    command.email = formValue.email || null;
    command.personalImage = personalImageBase64;
    command.commercialRegisterImage = commercialRegisterImageBase64;
    command.registerAs = formValue.registerAs;
    command.verificationBy = formValue.verificationBy;
    command.password = formValue.password || null;

    this.customerClient.create(command).subscribe({
      next: () => {
        this.router.navigate(['/main/customers']);
      },
      error: (error: any) => {
        this.errorMessage = error.error?.detail || error.error?.title || 'Failed to create customer. Please try again.';
        this.isSaving = false;
        console.error('Error creating customer:', error);
      }
    });
  }

  onCancel(): void {
    this.router.navigate(['/main/customers']);
  }
}

