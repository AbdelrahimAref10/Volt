import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VehicleClient, VehicleDto, PagedResultOfVehicleDto, CreateVehicleCommand, UpdateVehicleCommand, VehicleStatisticsDto } from '../../core/services/clientAPI';
import { CategoryClient, CategoryLookupDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.css'
})
export class VehiclesComponent implements OnInit {
  vehicles: VehicleDto[] = [];
  categories: CategoryLookupDto[] = [];
  statistics: VehicleStatisticsDto | null = null;
  
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  selectedCategoryId: number | null = null;
  selectedStatus: string | null = null;
  selectedCategoryName: string | null = null;
  
  isLoading = false;
  isLoadingStats = false;
  errorMessage = '';
  
  showModal = false;
  isEditMode = false;
  vehicleForm: FormGroup;
  selectedVehicleId: number | null = null;
  imagePreview: string | null = null;

  constructor(
    private vehicleClient: VehicleClient,
    private categoryClient: CategoryClient,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.vehicleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      categoryId: [null, [Validators.required]],
      pricePerHour: [0, [Validators.required, Validators.min(0)]],
      status: ['Available', [Validators.required]],
      imageUrl: [null]
    });
  }

  ngOnInit(): void {
    // Check for category filter from query params
    this.route.queryParams.subscribe(params => {
      if (params['categoryId']) {
        this.selectedCategoryId = +params['categoryId'];
        // Update category name after categories are loaded
        if (this.categories.length > 0) {
          const category = this.categories.find(c => c.categoryId === this.selectedCategoryId);
          this.selectedCategoryName = category ? category.name : null;
        }
      } else {
        this.selectedCategoryId = null;
        this.selectedCategoryName = null;
      }
    });

    this.loadCategories();
    this.loadStatistics();
    this.loadVehicles();
  }

  loadCategories(): void {
    this.categoryClient.getLookup().subscribe({
      next: (result) => {
        this.categories = result || [];
        // Find category name if categoryId is selected
        if (this.selectedCategoryId) {
          const category = this.categories.find(c => c.categoryId === this.selectedCategoryId);
          this.selectedCategoryName = category ? category.name : null;
        }
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadStatistics(): void {
    this.isLoadingStats = true;

    this.vehicleClient.getStatistics(this.selectedCategoryId || undefined).subscribe({
      next: (result: VehicleStatisticsDto) => {
        this.statistics = result;
        this.isLoadingStats = false;
      },
      error: (error) => {
        console.error('Error loading statistics:', error);
        this.isLoadingStats = false;
      }
    });
  }

  loadVehicles(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.vehicleClient.getAll(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      this.selectedCategoryId || undefined,
      this.selectedStatus || undefined
    ).subscribe({
      next: (result: PagedResultOfVehicleDto) => {
        this.vehicles = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load vehicles. Please try again.';
        this.isLoading = false;
        console.error('Error loading vehicles:', error);
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadVehicles();
  }

  onCategoryFilter(categoryId: number | null): void {
    this.selectedCategoryId = categoryId;
    if (categoryId) {
      const category = this.categories.find(c => c.categoryId === categoryId);
      this.selectedCategoryName = category ? category.name : null;
    } else {
      this.selectedCategoryName = null;
    }
    this.currentPage = 1;
    this.loadVehicles();
    this.loadStatistics();
  }

  onStatusFilter(status: string | null): void {
    this.selectedStatus = status;
    this.currentPage = 1;
    this.loadVehicles();
  }

  onViewDetails(filter: string): void {
    switch(filter) {
      case 'all':
        this.selectedCategoryId = null;
        this.selectedStatus = null;
        break;
      case 'available':
        this.selectedStatus = 'Available';
        break;
      case 'maintenance':
        this.selectedStatus = 'Under Maintenance';
        break;
      case 'new':
        // Filter for new this month - would need backend support
        this.selectedStatus = null;
        break;
    }
    this.currentPage = 1;
    this.loadVehicles();
  }

  onAddNew(): void {
    this.isEditMode = false;
    this.selectedVehicleId = null;
    this.vehicleForm.reset({
      status: 'Available',
      pricePerHour: 0
    });
    this.imagePreview = null;
    this.showModal = true;
  }

  onEdit(vehicle: VehicleDto): void {
    this.isEditMode = true;
    this.selectedVehicleId = vehicle.vehicleId;
    this.vehicleForm.patchValue({
      name: vehicle.name,
      categoryId: vehicle.categoryId,
      pricePerHour: vehicle.pricePerHour,
      status: vehicle.status,
      imageUrl: vehicle.imageUrl
    });
    this.imagePreview = vehicle.imageUrl || null;
    this.showModal = true;
  }

  onDelete(vehicleId: number): void {
    if (confirm('Are you sure you want to delete this vehicle?')) {
      this.vehicleClient.delete(vehicleId).subscribe({
        next: () => {
          this.loadVehicles();
          this.loadStatistics();
        },
        error: (error) => {
          alert('Failed to delete vehicle. Please try again.');
          console.error('Error deleting vehicle:', error);
        }
      });
    }
  }

  onImageSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
        // Convert to base64 for backend
        const base64 = e.target.result.split(',')[1];
        this.vehicleForm.patchValue({ imageUrl: `data:image/jpeg;base64,${base64}` });
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.vehicleForm.invalid) {
      this.vehicleForm.markAllAsTouched();
      return;
    }

    const formValue = this.vehicleForm.value;

    if (this.isEditMode && this.selectedVehicleId) {
      const command = new UpdateVehicleCommand();
      command.vehicleId = this.selectedVehicleId;
      command.name = formValue.name;
      command.categoryId = formValue.categoryId;
      command.pricePerHour = formValue.pricePerHour;
      command.status = formValue.status;
      command.imageUrl = formValue.imageUrl;

      this.vehicleClient.update(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadVehicles();
          this.loadStatistics();
        },
        error: (error) => {
          alert('Failed to update vehicle. Please try again.');
          console.error('Error updating vehicle:', error);
        }
      });
    } else {
      const command = new CreateVehicleCommand();
      command.name = formValue.name;
      command.categoryId = formValue.categoryId;
      command.pricePerHour = formValue.pricePerHour;
      command.status = formValue.status;
      command.imageUrl = formValue.imageUrl;

      this.vehicleClient.create(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadVehicles();
          this.loadStatistics();
        },
        error: (error) => {
          alert('Failed to create vehicle. Please try again.');
          console.error('Error creating vehicle:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.vehicleForm.reset();
    this.imagePreview = null;
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadVehicles();
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

  getStatusClass(status: string): string {
    switch(status) {
      case 'Available':
        return 'vehicles__status--available';
      case 'Under Maintenance':
        return 'vehicles__status--maintenance';
      case 'Rented':
        return 'vehicles__status--rented';
      default:
        return '';
    }
  }
}
