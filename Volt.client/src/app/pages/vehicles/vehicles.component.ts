import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { VehicleClient, VehicleDto, PagedResultOfVehicleDto, VehicleStatisticsDto } from '../../core/services/clientAPI';
import { SubCategoryClient, SubCategoryLookupDto } from '../../core/services/clientAPI';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-vehicles',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule, ConfirmDialogComponent],
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
  successMessage = '';
  
  // Confirmation dialog
  showConfirmDialog = false;
  confirmDialogTitle = '';
  confirmDialogMessage = '';
  confirmDialogType: 'danger' | 'warning' | 'info' = 'danger';
  confirmDialogLoading = false;
  pendingDeleteId: number | null = null;
  

  constructor(
    private vehicleClient: VehicleClient,
    private subCategoryClient: SubCategoryClient,
    private route: ActivatedRoute,
    private router: Router
  ) {}

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
    this.router.navigate(['/main/vehicles/new']);
  }

  onEdit(vehicle: VehicleDto): void {
    this.router.navigate(['/main/vehicles', vehicle.vehicleId, 'edit']);
  }

  onDelete(vehicleId: number): void {
    this.pendingDeleteId = vehicleId;
    this.confirmDialogTitle = 'Delete Vehicle';
    this.confirmDialogMessage = 'Are you sure you want to delete this vehicle? This action cannot be undone.';
    this.confirmDialogType = 'danger';
    this.showConfirmDialog = true;
  }

  onConfirmDelete(): void {
    if (this.pendingDeleteId === null) return;

    this.confirmDialogLoading = true;
    this.vehicleClient.delete(this.pendingDeleteId).subscribe({
      next: () => {
        this.showConfirmDialog = false;
        this.confirmDialogLoading = false;
        this.pendingDeleteId = null;
        this.showSuccessMessage('Vehicle deleted successfully');
        this.loadVehicles();
        this.loadStatistics();
      },
      error: (error: any) => {
        this.confirmDialogLoading = false;
        // Extract error message from backend - check errorMessage first (ProblemDetail structure)
        let errorMessage = 'Failed to delete vehicle. Please try again.';
        if (error.error) {
          if (error.error.errorMessage) {
            errorMessage = error.error.errorMessage;
          } else if (error.error.detail) {
            errorMessage = error.error.detail;
          } else if (error.error.title) {
            errorMessage = error.error.title;
          } else if (typeof error.error === 'string') {
            errorMessage = error.error;
          }
        } else if (error.message) {
          errorMessage = error.message;
        }
        this.showErrorMessage(errorMessage);
        this.showConfirmDialog = false;
        this.pendingDeleteId = null;
        console.error('Error deleting vehicle:', error);
      }
    });
  }

  onCancelDelete(): void {
    this.showConfirmDialog = false;
    this.confirmDialogLoading = false;
    this.pendingDeleteId = null;
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
