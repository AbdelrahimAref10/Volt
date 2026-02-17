import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { VehicleClient, VehicleDto, CreateVehicleCommand, UpdateVehicleCommand } from '../../../core/services/clientAPI';
import { SubCategoryClient, SubCategoryLookupDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-vehicle-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './vehicle-form.component.html',
  styleUrl: './vehicle-form.component.css'
})
export class VehicleFormComponent implements OnInit {
  vehicleForm: FormGroup;
  isEditMode = false;
  vehicleId: number | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  subCategories: SubCategoryLookupDto[] = [];
  imagePreview: string | null = null;
  selectedImageFile: File | null = null;

  constructor(
    private vehicleClient: VehicleClient,
    private subCategoryClient: SubCategoryClient,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.vehicleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      vehicleCode: ['', [Validators.required, Validators.minLength(1)]],
      subCategoryId: [null, [Validators.required]],
      status: ['Available', [Validators.required]],
      imageUrl: [null]
    });
  }

  ngOnInit(): void {
    this.loadSubCategories();
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id && id !== 'new') {
        this.vehicleId = +id;
        this.isEditMode = true;
        this.loadVehicle();
      } else {
        this.isEditMode = false;
        this.vehicleId = null;
      }
    });
  }

  loadSubCategories(): void {
    this.subCategoryClient.getLookup().subscribe({
      next: (result) => {
        this.subCategories = result || [];
      },
      error: (error) => {
        console.error('Error loading subcategories:', error);
      }
    });
  }

  loadVehicle(): void {
    if (!this.vehicleId) return;

    this.isLoading = true;
    this.vehicleClient.getById(this.vehicleId).subscribe({
      next: (vehicle: VehicleDto) => {
        this.vehicleForm.patchValue({
          name: vehicle.name,
          vehicleCode: vehicle.vehicleCode,
          subCategoryId: vehicle.subCategoryId,
          status: vehicle.status,
          imageUrl: vehicle.imageUrl
        });

        if (vehicle.imageUrl) {
          this.imagePreview = vehicle.imageUrl;
        }

        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load vehicle. Please try again.';
        this.isLoading = false;
        console.error('Error loading vehicle:', error);
      }
    });
  }

  onImageSelect(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      this.selectedImageFile = file;

      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imagePreview = e.target.result;
        this.vehicleForm.patchValue({ imageUrl: e.target.result });
      };
      reader.readAsDataURL(file);
    }
  }

  removeImage(): void {
    this.imagePreview = null;
    this.selectedImageFile = null;
    this.vehicleForm.patchValue({ imageUrl: null });
  }

  onSubmit(): void {
    if (this.vehicleForm.invalid) {
      this.vehicleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formValue = this.vehicleForm.value;

    if (this.isEditMode && this.vehicleId) {
      const command = new UpdateVehicleCommand();
      command.vehicleId = this.vehicleId;
      command.name = formValue.name;
      command.vehicleCode = formValue.vehicleCode;
      command.subCategoryId = formValue.subCategoryId;
      command.status = formValue.status;
      // Only send imageUrl if it's a new base64 image (starts with data:image/), otherwise send null
      command.imageUrl = this.selectedImageFile ? formValue.imageUrl : null;

      this.vehicleClient.update(command).subscribe({
        next: () => {
          this.router.navigate(['/main/vehicles']);
        },
        error: (error: any) => {
          this.errorMessage = error.error?.detail || error.error?.title || 'Failed to update vehicle. Please try again.';
          this.isSaving = false;
          console.error('Error updating vehicle:', error);
        }
      });
    } else {
      const command = new CreateVehicleCommand();
      command.name = formValue.name;
      command.vehicleCode = formValue.vehicleCode;
      command.subCategoryId = formValue.subCategoryId;
      command.status = formValue.status;
      command.imageUrl = formValue.imageUrl;

      this.vehicleClient.create(command).subscribe({
        next: () => {
          this.router.navigate(['/main/vehicles']);
        },
        error: (error: any) => {
          this.errorMessage = error.error?.detail || error.error?.title || 'Failed to create vehicle. Please try again.';
          this.isSaving = false;
          console.error('Error creating vehicle:', error);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/main/vehicles']);
  }
}

