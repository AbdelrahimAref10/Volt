import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { CategoryClient, CategoryDto, PagedResultOfCategoryDto } from '../../core/services/clientAPI';
import { ConfirmDialogComponent } from '../../shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, RouterModule, ConfirmDialogComponent],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css'
})
export class CategoriesComponent implements OnInit {
  categories: CategoryDto[] = [];
  currentPage = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;
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
    private categoryClient: CategoryClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCategories();
  }

  loadCategories(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.categoryClient.getAll(this.currentPage, this.pageSize).subscribe({
      next: (result: PagedResultOfCategoryDto) => {
        this.categories = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error) => {
        this.errorMessage = 'Failed to load categories. Please try again.';
        this.isLoading = false;
        console.error('Error loading categories:', error);
      }
    });
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.loadCategories();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  onDelete(categoryId: number): void {
    this.pendingDeleteId = categoryId;
    this.pendingDeactivateId = null;
    this.pendingAction = 'delete';
    this.confirmDialogTitle = 'Permanently Delete Category';
    this.confirmDialogMessage = 'Are you sure you want to permanently delete this category? This action cannot be undone. The category must be inactive first.';
    this.confirmDialogType = 'danger';
    this.showConfirmDialog = true;
  }

  onDeactivate(categoryId: number): void {
    this.pendingDeactivateId = categoryId;
    this.pendingDeleteId = null;
    this.pendingActivateId = null;
    this.pendingAction = 'deactivate';
    this.confirmDialogTitle = 'Deactivate Category';
    this.confirmDialogMessage = 'Are you sure you want to deactivate this category? It will be moved to inactive categories.';
    this.confirmDialogType = 'warning';
    this.showConfirmDialog = true;
  }

  onActivate(categoryId: number): void {
    this.pendingActivateId = categoryId;
    this.pendingDeleteId = null;
    this.pendingDeactivateId = null;
    this.pendingAction = 'activate';
    this.confirmDialogTitle = 'Activate Category';
    this.confirmDialogMessage = 'Are you sure you want to activate this category? It will be moved to active categories.';
    this.confirmDialogType = 'info';
    this.showConfirmDialog = true;
  }

  onConfirmAction(): void {
    if (this.pendingAction === 'delete' && this.pendingDeleteId !== null) {
      this.confirmDialogLoading = true;
      this.categoryClient.delete(this.pendingDeleteId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingDeleteId = null;
          this.pendingAction = null;
          this.showSuccessMessage('Category deleted successfully');
          this.loadCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          const errorMessage = this.extractErrorMessage(error) || 'Failed to delete category. Please try again.';
          this.showErrorMessage(errorMessage);
          this.showConfirmDialog = false;
          this.pendingDeleteId = null;
          this.pendingAction = null;
          console.error('Error deleting category:', error);
        }
      });
    } else if (this.pendingAction === 'deactivate' && this.pendingDeactivateId !== null) {
      this.confirmDialogLoading = true;
      this.categoryClient.deactivate(this.pendingDeactivateId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingDeactivateId = null;
          this.pendingAction = null;
          this.showSuccessMessage('Category deactivated successfully');
          this.loadCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          const errorMessage = this.extractErrorMessage(error) || 'Failed to deactivate category. Please try again.';
          this.showErrorMessage(errorMessage);
          this.showConfirmDialog = false;
          this.pendingDeactivateId = null;
          this.pendingAction = null;
          console.error('Error deactivating category:', error);
        }
      });
    } else if (this.pendingAction === 'activate' && this.pendingActivateId !== null) {
      this.confirmDialogLoading = true;
      this.categoryClient.activate(this.pendingActivateId).subscribe({
        next: () => {
          this.showConfirmDialog = false;
          this.confirmDialogLoading = false;
          this.pendingActivateId = null;
          this.pendingAction = null;
          this.showSuccessMessage('Category activated successfully');
          this.loadCategories();
        },
        error: (error: any) => {
          this.confirmDialogLoading = false;
          const errorMessage = this.extractErrorMessage(error) || 'Failed to activate category. Please try again.';
          this.showErrorMessage(errorMessage);
          this.showConfirmDialog = false;
          this.pendingActivateId = null;
          this.pendingAction = null;
          console.error('Error activating category:', error);
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

  onViewSubCategories(categoryId: number): void {
    // Navigate to subcategories page filtered by category
    this.router.navigate(['/main/subcategories'], { queryParams: { categoryId: categoryId } });
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
    this.router.navigate(['/main/categories/new']);
  }

  onEdit(category: CategoryDto): void {
    this.router.navigate(['/main/categories', category.categoryId, 'edit']);
  }
}
