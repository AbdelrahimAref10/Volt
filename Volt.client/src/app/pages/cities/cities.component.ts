import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { CityClient, CityDto, PagedResultOfCityDto, AddCityCommand, UpdateCityCommand } from '../../core/services/clientAPI';

@Component({
  selector: 'app-cities',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
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
  activeTab: 'active' | 'inactive' = 'active';

  showModal = false;
  isEditMode = false;
  cityForm: FormGroup;
  selectedCityId: number | null = null;

  constructor(
    private cityClient: CityClient,
    private http: HttpClient,
    private fb: FormBuilder
  ) {
    this.cityForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [null]
    });
  }

  ngOnInit(): void {
    this.loadCities();
  }

  loadCities(): void {
    this.isLoading = true;
    this.errorMessage = '';

    const isActive = this.activeTab === 'active' ? true : false;
    const url = `/api/City?PageNumber=${this.currentPage}&PageSize=${this.pageSize}&IsActive=${isActive}${this.searchTerm ? '&SearchTerm=' + encodeURIComponent(this.searchTerm) : ''}`;

    this.http.get<PagedResultOfCityDto>(url).subscribe({
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

  onAddNew(): void {
    this.isEditMode = false;
    this.selectedCityId = null;
    this.cityForm.reset();
    this.showModal = true;
  }

  onEdit(city: CityDto): void {
    this.isEditMode = true;
    this.selectedCityId = city.cityId;
    this.cityForm.patchValue({
      name: city.name,
      description: city.description
    });
    this.showModal = true;
  }

  onDelete(cityId: number): void {
    if (confirm('Are you sure you want to deactivate this city? It will be moved to inactive cities.')) {
      const url = `/api/City/${cityId}/deactivate`;
      this.http.post<boolean>(url, {}).subscribe({
        next: () => {
          this.loadCities();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to deactivate city. Please try again.';
          alert(errorMessage);
          console.error('Error deactivating city:', error);
        }
      });
    }
  }

  onActivate(cityId: number): void {
    if (confirm('Are you sure you want to reactivate this city?')) {
      const url = `/api/City/${cityId}/activate`;
      this.http.post<number>(url, {}).subscribe({
        next: () => {
          this.loadCities();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to activate city. Please try again.';
          alert(errorMessage);
          console.error('Error activating city:', error);
        }
      });
    }
  }

  onPermanentlyDelete(cityId: number): void {
    if (confirm('Are you sure you want to permanently delete this city? This action cannot be undone and the city must have no customers.')) {
      const url = `/api/City/${cityId}/permanent`;
      this.http.delete<boolean>(url).subscribe({
        next: () => {
          this.loadCities();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to permanently delete city. Please try again.';
          alert(errorMessage);
          console.error('Error permanently deleting city:', error);
        }
      });
    }
  }

  onSubmit(): void {
    if (this.cityForm.invalid) {
      this.cityForm.markAllAsTouched();
      return;
    }

    const formValue = this.cityForm.value;

    if (this.isEditMode && this.selectedCityId) {
      const command = new UpdateCityCommand();
      command.cityId = this.selectedCityId;
      command.name = formValue.name;
      command.description = formValue.description || null;

      this.cityClient.update(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCities();
        },
        error: (error: any) => {
          alert('Failed to update city. Please try again.');
          console.error('Error updating city:', error);
        }
      });
    } else {
      const command = new AddCityCommand();
      command.name = formValue.name;
      command.description = formValue.description || null;

      this.cityClient.add(command).subscribe({
        next: () => {
          this.showModal = false;
          this.loadCities();
        },
        error: (error: any) => {
          alert('Failed to add city. Please try again.');
          console.error('Error adding city:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.cityForm.reset();
    this.selectedCityId = null;
    this.isEditMode = false;
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

