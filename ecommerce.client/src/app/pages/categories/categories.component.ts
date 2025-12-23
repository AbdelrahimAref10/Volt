import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CategoryClient, CategoryDto, PagedResultOfCategoryDto, CreateCategoryCommand, UpdateCategoryCommand } from '../../core/services/clientAPI';
import { CityClient, CityDto, PagedResultOfCityDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-categories',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './categories.component.html',
  styleUrl: './categories.component.css'
})
export class CategoriesComponent implements OnInit {
  categories: CategoryDto[] = [];
  cities: CityDto[] = [];
  currentPage = 1;
  pageSize = 12;
  totalCount = 0;
  totalPages = 0;
  isLoading = false;
  errorMessage = '';
  
  showModal = false;
  isEditMode = false;
  categoryForm: FormGroup;
  selectedCategoryId: number | null = null;
  imagePreview: string | null = null;

  constructor(
    private categoryClient: CategoryClient,
    private cityClient: CityClient,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.categoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['', [Validators.required]],
      cityId: [null, [Validators.required]],
      imageUrl: [null]
    });
  }

  ngOnInit(): void {
    this.loadCities();
    this.loadCategories();
  }

  loadCities(): void {
    // Load all active cities for the dropdown
    this.cityClient.getAll(1, 1000, undefined, true).subscribe({
      next: (result: PagedResultOfCityDto) => {
        this.cities = result.items || [];
      },
      error: (error) => {
        console.error('Error loading cities:', error);
      }
    });
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
    if (confirm('Are you sure you want to delete this category?')) {
      this.categoryClient.delete(categoryId).subscribe({
        next: () => {
          this.loadCategories();
        },
        error: (error) => {
          alert('Failed to delete category. Please try again.');
          console.error('Error deleting category:', error);
        }
      });
    }
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
    this.isEditMode = false;
    this.selectedCategoryId = null;
    this.categoryForm.reset();
    this.imagePreview = null;
    this.showModal = true;
  }

  onEdit(category: CategoryDto): void {
    this.isEditMode = true;
    this.selectedCategoryId = category.categoryId;
    this.categoryForm.patchValue({
      name: category.name,
      description: category.description,
      cityId: category.cityId,
      imageUrl: category.imageUrl
    });
    this.imagePreview = category.imageUrl || null;
    this.showModal = true;
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
        this.categoryForm.patchValue({ imageUrl: `data:image/jpeg;base64,${base64}` });
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.categoryForm.invalid) {
      this.categoryForm.markAllAsTouched();
      return;
    }

    const formValue = this.categoryForm.value;

    if (this.isEditMode && this.selectedCategoryId) {
      const command = new UpdateCategoryCommand();
      command.categoryId = this.selectedCategoryId;
      command.name = formValue.name;
      command.description = formValue.description;
      command.cityId = formValue.cityId;
      command.imageUrl = formValue.imageUrl;

      this.categoryClient.update(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCategories();
        },
        error: (error) => {
          alert('Failed to update category. Please try again.');
          console.error('Error updating category:', error);
        }
      });
    } else {
      const command = new CreateCategoryCommand();
      command.name = formValue.name;
      command.description = formValue.description;
      command.cityId = formValue.cityId;
      command.imageUrl = formValue.imageUrl;

      this.categoryClient.create(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCategories();
        },
        error: (error) => {
          alert('Failed to create category. Please try again.');
          console.error('Error creating category:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.categoryForm.reset();
    this.imagePreview = null;
  }
}
