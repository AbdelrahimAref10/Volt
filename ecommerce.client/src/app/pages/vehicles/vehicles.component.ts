import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { VehicleClient, VehicleDto, PagedResultOfVehicleDto, CreateVehicleCommand, UpdateVehicleCommand, VehicleStatisticsDto } from '../../core/services/clientAPI';
import { SubCategoryClient, SubCategoryLookupDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './vehicles.component.html',
  styleUrl: './vehicles.component.css'
})
export class VehiclesComponent implements OnInit {
  vehicles: VehicleDto[] = [];
  subCategories: SubCategoryLookupDto[] = [];
  statistics: VehicleStatisticsDto | null = null;
  
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  selectedSubCategoryId: number | null = null;
  selectedStatus: string | null = null;
  selectedSubCategoryName: string | null = null;
  
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
    private subCategoryClient: SubCategoryClient,
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router
  ) {
    this.vehicleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      subCategoryId: [null, [Validators.required]],
      status: ['Available', [Validators.required]],
      imageUrl: [null]
    });
  }

  ngOnInit(): void {
    // Check for subcategory filter from query params
    this.route.queryParams.subscribe(params => {
      if (params['subCategoryId']) {
        this.selectedSubCategoryId = +params['subCategoryId'];
      } else {
        this.selectedSubCategoryId = null;
        this.selectedSubCategoryName = null;
      }
      // Reload data when params change
      this.loadStatistics();
      this.loadVehicles();
    });

    // Initial load
    this.loadSubCategories();
    this.loadStatistics();
    this.loadVehicles();
  }

  loadSubCategories(): void {
    this.subCategoryClient.getLookup().subscribe({
      next: (result) => {
        this.subCategories = result || [];
        // Find subcategory name if subCategoryId is selected
        if (this.selectedSubCategoryId) {
          const subCategory = this.subCategories.find(sc => sc.subCategoryId === this.selectedSubCategoryId);
          this.selectedSubCategoryName = subCategory ? subCategory.name : null;
        }
      },
      error: (error) => {
        console.error('Error loading subcategories:', error);
      }
    });
  }

  loadStatistics(): void {
    this.isLoadingStats = true;

    this.vehicleClient.getStatistics(undefined, this.selectedSubCategoryId || undefined).subscribe({
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
      undefined, // categoryId
      this.selectedSubCategoryId || undefined,
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

  onSubCategoryFilter(subCategoryId: number | null): void {
    this.selectedSubCategoryId = subCategoryId;
    if (subCategoryId) {
      const subCategory = this.subCategories.find(sc => sc.subCategoryId === subCategoryId);
      this.selectedSubCategoryName = subCategory ? subCategory.name : null;
    } else {
      this.selectedSubCategoryName = null;
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
        this.selectedSubCategoryId = null;
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
      status: 'Available'
    });
    this.imagePreview = null;
    this.showModal = true;
  }

  onEdit(vehicle: VehicleDto): void {
    this.isEditMode = true;
    this.selectedVehicleId = vehicle.vehicleId;
    this.vehicleForm.patchValue({
      name: vehicle.name,
      subCategoryId: vehicle.subCategoryId,
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
      command.subCategoryId = formValue.subCategoryId;
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
      command.subCategoryId = formValue.subCategoryId;
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
