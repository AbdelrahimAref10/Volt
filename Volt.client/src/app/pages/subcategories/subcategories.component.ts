import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { SubCategoryClient, SubCategoryDto, PagedResultOfSubCategoryDto, CreateSubCategoryCommand, UpdateSubCategoryCommand } from '../../core/services/clientAPI';
import { CategoryClient, CategoryLookupDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-subcategories',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
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
  
  showModal = false;
  isEditMode = false;
  subCategoryForm: FormGroup;
  selectedSubCategoryId: number | null = null;
  imagePreview: string | null = null;

  constructor(
    private subCategoryClient: SubCategoryClient,
    private categoryClient: CategoryClient,
    private router: Router,
    private route: ActivatedRoute,
    private fb: FormBuilder
  ) {
    this.subCategoryForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: ['', [Validators.required]],
      categoryId: [null, [Validators.required]],
      price: [0, [Validators.required, Validators.min(0)]],
      isOffer: [false],
      imageUrl: [null]
    });
  }

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
    if (confirm('Are you sure you want to delete this subcategory? This will also delete all associated vehicles.')) {
      this.subCategoryClient.delete(subCategoryId).subscribe({
        next: () => {
          this.loadSubCategories();
        },
        error: (error) => {
          alert('Failed to delete subcategory. Please try again.');
          console.error('Error deleting subcategory:', error);
        }
      });
    }
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
    this.isEditMode = false;
    this.selectedSubCategoryId = null;
    this.subCategoryForm.reset({
      price: 0,
      isOffer: false
    });
    this.imagePreview = null;
    this.showModal = true;
  }

  onEdit(subCategory: SubCategoryDto): void {
    this.isEditMode = true;
    this.selectedSubCategoryId = subCategory.subCategoryId;
    this.subCategoryForm.patchValue({
      name: subCategory.name,
      description: subCategory.description,
      categoryId: subCategory.categoryId,
      price: subCategory.price,
      isOffer: subCategory.isOffer,
      imageUrl: subCategory.imageUrl
    });
    this.imagePreview = subCategory.imageUrl || null;
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
        this.subCategoryForm.patchValue({ imageUrl: `data:image/jpeg;base64,${base64}` });
      };
      reader.readAsDataURL(file);
    }
  }

  onSubmit(): void {
    if (this.subCategoryForm.invalid) {
      this.subCategoryForm.markAllAsTouched();
      return;
    }

    const formValue = this.subCategoryForm.value;

    if (this.isEditMode && this.selectedSubCategoryId) {
      const command = new UpdateSubCategoryCommand();
      command.subCategoryId = this.selectedSubCategoryId;
      command.name = formValue.name;
      command.description = formValue.description;
      command.categoryId = formValue.categoryId;
      command.price = formValue.price;
      command.isOffer = formValue.isOffer || false;
      command.imageUrl = formValue.imageUrl;

      this.subCategoryClient.update(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadSubCategories();
        },
        error: (error) => {
          alert('Failed to update subcategory. Please try again.');
          console.error('Error updating subcategory:', error);
        }
      });
    } else {
      const command = new CreateSubCategoryCommand();
      command.name = formValue.name;
      command.description = formValue.description;
      command.categoryId = formValue.categoryId;
      command.price = formValue.price;
      command.isOffer = formValue.isOffer || false;
      command.imageUrl = formValue.imageUrl;

      this.subCategoryClient.create(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadSubCategories();
        },
        error: (error) => {
          alert('Failed to create subcategory. Please try again.');
          console.error('Error creating subcategory:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.subCategoryForm.reset();
    this.imagePreview = null;
  }
}

