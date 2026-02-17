import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CityClient, CityDto, PagedResultOfCityDto } from '../../core/services/clientAPI';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-cities',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ConfirmDialogComponent],
  templateUrl: './cities.component.html',
  styleUrl: './cities.component.css'
})
export class CitiesComponent implements OnInit {
  cities: CityDto[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  activeTab: 'active' | 'inactive' = 'active';

  // Confirmation dialog
  showConfirmDialog = false;
  confirmDialogTitle = '';
  confirmDialogMessage = '';
  confirmDialogType: 'danger' | 'warning' | 'info' = 'danger';
  confirmDialogLoading = false;
  pendingAction: 'deactivate' | 'activate' | 'permanentDelete' | null = null;
  pendingCityId: number | null = null;

  constructor(
    private cityClient: CityClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCities();
  }

  loadCities(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const isActive = this.activeTab === 'active' ? true : false;
    this.cityClient.getAll(
      this.currentPage,
      this.pageSize,
      this.searchTerm || undefined,
      isActive
    ).subscribe({
      next: (result: PagedResultOfCityDto) => {
        this.cities = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load cities. Please try again.';
        this.isLoading = false;
        console.error('Error loading cities:', error);
      }
    });
  }

  onTabChange(tab: 'active' | 'inactive'): void {
    this.activeTab = tab;
    this.currentPage = 1;
    this.loadCities();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadCities();
  }

  onSearchKeyUp(event: KeyboardEvent): void {
    if (event.key === 'Enter') {
      this.onSearch();
    }
  }

  onAddNew(): void {
    this.router.navigate(['/main/cities/new']);
  }

  onEdit(city: CityDto): void {
    this.router.navigate(['/main/cities', city.cityId, 'edit']);
  }

  onDelete(cityId: number): void {
    this.pendingCityId = cityId;
    this.pendingAction = 'deactivate';
    this.confirmDialogTitle = 'Deactivate City';
    this.confirmDialogMessage = 'Are you sure you want to deactivate this city? It will be moved to inactive cities.';
    this.confirmDialogType = 'warning';
    this.showConfirmDialog = true;
  }

  onActivate(cityId: number): void {
    this.pendingCityId = cityId;
    this.pendingAction = 'activate';
    this.confirmDialogTitle = 'Activate City';
    this.confirmDialogMessage = 'Are you sure you want to reactivate this city?';
    this.confirmDialogType = 'info';
    this.showConfirmDialog = true;
  }

  onPermanentlyDelete(cityId: number): void {
    this.pendingCityId = cityId;
    this.pendingAction = 'permanentDelete';
    this.confirmDialogTitle = 'Permanently Delete City';
    this.confirmDialogMessage = 'Are you sure you want to permanently delete this city? This action cannot be undone and the city must have no customers.';
    this.confirmDialogType = 'danger';
    this.showConfirmDialog = true;
  }

  onConfirmAction(): void {
    if (this.pendingCityId === null || this.pendingAction === null) return;

    this.confirmDialogLoading = true;
    const action = this.pendingAction;
    const cityId = this.pendingCityId;
    let successMessage = '';

    switch (action) {
      case 'deactivate':
        successMessage = 'City deactivated successfully';
        this.cityClient.deactivate(cityId).subscribe({
          next: () => {
            this.showConfirmDialog = false;
            this.confirmDialogLoading = false;
            this.pendingCityId = null;
            this.pendingAction = null;
            this.showSuccessMessage(successMessage);
            this.loadCities();
          },
          error: (error: any) => {
            this.confirmDialogLoading = false;
            let errorMessage = error.errorMessage;
            this.showErrorMessage(errorMessage);
            this.showConfirmDialog = false;
            this.pendingCityId = null;
            this.pendingAction = null;
            console.error('Error deactivating city:', error);
          }
        });
        break;
      case 'activate':
        successMessage = 'City activated successfully';
        this.cityClient.activate(cityId).subscribe({
          next: () => {
            this.showConfirmDialog = false;
            this.confirmDialogLoading = false;
            this.pendingCityId = null;
            this.pendingAction = null;
            this.showSuccessMessage(successMessage);
            this.loadCities();
          },
          error: (error: any) => {
            this.confirmDialogLoading = false;
            // Extract error message from backend - check errorMessage first (ProblemDetail structure)
            let errorMessage = 'Failed to activate city. Please try again.';
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
            this.pendingCityId = null;
            this.pendingAction = null;
            console.error('Error activating city:', error);
          }
        });
        break;
      case 'permanentDelete':
        successMessage = 'City permanently deleted successfully';
        this.cityClient.permanentlyDelete(cityId).subscribe({
          next: () => {
            this.showConfirmDialog = false;
            this.confirmDialogLoading = false;
            this.pendingCityId = null;
            this.pendingAction = null;
            this.showSuccessMessage(successMessage);
            this.loadCities();
          },
          error: (error: any) => {
            this.confirmDialogLoading = false;
            // Extract error message from backend - check errorMessage first (ProblemDetail structure)
            let errorMessage = 'Failed to permanently delete city. Please try again.';
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
            this.pendingCityId = null;
            this.pendingAction = null;
            console.error('Error permanently deleting city:', error);
          }
        });
        break;
      default:
        this.confirmDialogLoading = false;
        return;
    }
  }

  onCancelAction(): void {
    this.showConfirmDialog = false;
    this.confirmDialogLoading = false;
    this.pendingCityId = null;
    this.pendingAction = null;
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
      this.loadCities();
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
}

