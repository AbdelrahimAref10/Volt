import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { SubCategoryClient, SubCategoryDto, PagedResultOfSubCategoryDto } from '../../core/services/clientAPI';
import { CategoryClient, CategoryLookupDto } from '../../core/services/clientAPI';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-subcategories',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ConfirmDialogComponent],
  templateUrl: './subcategories.component.html',
  styleUrl: './subcategories.component.css'
})
export class SubCategoriesComponent implements OnInit {
  subCategories: SubCategoryDto[] = [];
  categories: CategoryLookupDto[] = [];
  currentPage = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  selectedCategoryId: number | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  // Confirmation dialog
  showConfirmDialog = false;
  confirmDialogTitle = '';
  confirmDialogMessage = '';
  confirmDialogType: 'danger' | 'warning' | 'info' = 'danger';
  confirmDialogLoading = false;
  pendingDeleteId: number | null = null;
  pendingDeactivateId: number | null = null;
  pendingActivateId: number | null = null;
  pendingAction: 'delete' | 'deactivate' | 'activate' | null = null;

  constructor(
    private subCategoryClient: SubCategoryClient,
    private categoryClient: CategoryClient,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    // Check for category filter from query params
    this.route.queryParams.subscribe(params => {
      if (params['categoryId']) {
        this.selectedCategoryId = +params['categoryId'];
      } else {
        this.selectedCategoryId = null;
      }
    });

    this.loadCategories();
    this.loadSubCategories();
  }

  loadCategories(): void {
    this.categoryClient.getLookup().subscribe({
      next: (result) => {
        this.categories = result || [];
      },
      error: (error) => {
        console.error('Error loading categories:', error);
      }
    });
  }

  loadSubCategories(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.subCategoryClient.getAll(
      this.currentPage,
      this.pageSize,
      this.selectedCategoryId || undefined,
      this.searchTerm || undefined
    ).subscribe({
      next: (result: PagedResultOfSubCategoryDto) => {
        this.subCategories = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load subcategories. Please try again.';
        this.isLoading = false;
        console.error('Error loading subcategories:', error);
      }
    });
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadSubCategories();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  onCategoryFilter(categoryId: number | null): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.loadSubCategories();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadSubCategories();
  }

  onDelete(subCategoryId: number): void {
    this.pendingDeleteId = subCategoryId;
    this.pendingDeactivateId = null;
    this.pendingAction = 'delete';
    this.confirmDialogTitle = 'Permanently Delete SubCategory';
    this.confirmDialogMessage = 'Are you sure you want to permanently delete this subcategory? This action cannot be undone. The subcategory must be inactive first.';
    this.confirmDialogType = 'danger';
    this.showConfirmDialog = true;
  }

  onDeactivate(subCategoryId: number): void {
    this.pendingDeactivateId = subCategoryId;
    this.pendingDeleteId = null;
    this.pendingActivateId = null;
    this.pendingAction = 'deactivate';
    this.confirmDialogTitle = 'Deactivate SubCategory';
    this.confirmDialogMessage = 'Are you sure you want to deactivate this subcategory? It will be moved to inactive subcategories.';
    this.confirmDialogType = 'warning';
    this.showConfirmDialog = true;
  }

  onActivate(subCategoryId: number): void {
    this.pendingActivateId = subCategoryId;
    this.pendingDeleteId = null;
    this.pendingDeactivateId = null;
    this.pendingAction = 'activate';
    this.confirmDialogTitle = 'Activate SubCategory';
    this.confirmDialogMessage = 'Are you sure you want to activate this subcategory? It will be moved to active subcategories.';
    this.confirmDialogType = 'info';
    this.showConfirmDialog = true;
  }

  onConfirmAction(): void {
    if (this.pendingAction === 'delete' && this.pendingDeleteId !== null) {
      this.confirmDialogLoading = true;
      this.subCategoryClient.delete(this.pendingDeleteId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingDeleteId = null;
          this.pendingAction = null;
          this.showSuccessMessage('SubCategory deleted successfully');
          this.loadSubCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          // Extract error message from backend - check errorMessage first (ProblemDetail structure)
          let errorMessage = 'Failed to delete subcategory. Please try again.';
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
          this.pendingAction = null;
          console.error('Error deleting subcategory:', error);
        }
      });
    } else if (this.pendingAction === 'deactivate' && this.pendingDeactivateId !== null) {
      this.confirmDialogLoading = true;
      this.subCategoryClient.deactivate(this.pendingDeactivateId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingDeactivateId = null;
          this.pendingAction = null;
          this.showSuccessMessage('SubCategory deactivated successfully');
          this.loadSubCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          const errorMessage = this.extractErrorMessage(error) || 'Failed to deactivate subcategory. Please try again.';
          this.showErrorMessage(errorMessage);
          this.showConfirmDialog = false;
          this.pendingDeactivateId = null;
          this.pendingAction = null;
          console.error('Error deactivating subcategory:', error);
        }
      });
    } else if (this.pendingAction === 'activate' && this.pendingActivateId !== null) {
      this.confirmDialogLoading = true;
      this.subCategoryClient.activate(this.pendingActivateId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingActivateId = null;
          this.pendingAction = null;
          this.showSuccessMessage('SubCategory activated successfully');
          this.loadSubCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          const errorMessage = this.extractErrorMessage(error) || 'Failed to activate subcategory. Please try again.';
          this.showErrorMessage(errorMessage);
          this.showConfirmDialog = false;
          this.pendingActivateId = null;
          this.pendingAction = null;
          console.error('Error activating subcategory:', error);
        }
      });
    }
  }

  onCancelAction(): void {
    this.showConfirmDialog = false;
    this.confirmDialogLoading = false;
    this.pendingDeleteId = null;
    this.pendingDeactivateId = null;
    this.pendingActivateId = null;
    this.pendingAction = null;
  }

  extractErrorMessage(error: any): string {
    if (error.error) {
      if (error.error.errorMessage) {
        return error.error.errorMessage;
      } else if (error.error.detail) {
        return error.error.detail;
      } else if (error.error.title) {
        return error.error.title;
      } else if (typeof error.error === 'string') {
        return error.error;
      }
    } else if (error.message) {
      return error.message;
    }
    return '';
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

  onViewVehicles(subCategoryId: number): void {
    // Navigate to vehicles page filtered by subcategory
    this.router.navigate(['/main/vehicles'], { queryParams: { subCategoryId: subCategoryId } });
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

  onAddNew(): void {
    this.router.navigate(['/main/subcategories/new']);
  }

  onEdit(subCategory: SubCategoryDto): void {
    this.router.navigate(['/main/subcategories', subCategory.subCategoryId, 'edit']);
  }
}

