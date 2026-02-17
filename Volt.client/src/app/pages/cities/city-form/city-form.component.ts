import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CityClient, CityDto, AddCityCommand, UpdateCityCommand, TieredDiscountDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-city-form',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './city-form.component.html',
  styleUrl: './city-form.component.css'
})
export class CityFormComponent implements OnInit {
  cityForm: FormGroup;
  isEditMode = false;
  cityId: number | null = null;
  isLoading = false;
  isSaving = false;
  errorMessage = '';

  constructor(
    private cityClient: CityClient,
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.cityForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2)]],
      description: [null],
      deliveryFees: [0, [Validators.min(0)]],
      urgentDelivery: [0, [Validators.min(0)]],
      serviceFees: [0, [Validators.min(0)]],
      cancellationFees: [0, [Validators.min(0), Validators.max(100)]],
      tieredDiscounts: this.fb.array([])
    });
  }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = params.get('id');
      if (id && id !== 'new') {
        this.cityId = +id;
        this.isEditMode = true;
        this.loadCity();
      } else {
        this.isEditMode = false;
        this.cityId = null;
      }
    });
  }

  get tieredDiscountsFormArray(): FormArray {
    return this.cityForm.get('tieredDiscounts') as FormArray;
  }

  getTieredDiscountFormGroup(index: number): FormGroup {
    return this.tieredDiscountsFormArray.at(index) as FormGroup;
  }

  addTieredDiscount(from: number = 0, to: number = 0, discount: number = 0, id: number = 0): void {
    const tieredDiscountForm = this.fb.group({
      id: [id], // Store the tier ID for reference (not used in validation)
      from: [from, [Validators.min(0)]],
      to: [to, [Validators.min(0)]],
      discount: [discount, [Validators.min(0), Validators.max(100)]]
    }, {
      validators: (formGroup: FormGroup) => {
        const fromValue = formGroup.get('from')?.value ?? 0;
        const toValue = formGroup.get('to')?.value ?? 0;
        const discountValue = formGroup.get('discount')?.value ?? 0;

        // Clear previous errors first
        formGroup.get('from')?.setErrors(null);
        formGroup.get('to')?.setErrors(null);
        formGroup.get('discount')?.setErrors(null);

        // If all values are 0 or empty, tier is empty and valid (optional)
        const isEmpty = (!fromValue || fromValue === 0) && (!toValue || toValue === 0) && (!discountValue || discountValue === 0);
        if (isEmpty) {
          return null; // Empty tier is valid
        }

        // If any field has a value, all fields become required
        const hasAnyValue = (fromValue && fromValue > 0) || (toValue && toValue > 0) || (discountValue && discountValue > 0);

        if (hasAnyValue) {
          let hasErrors = false;

          // If any field has value, all must be filled
          if (!fromValue || fromValue <= 0) {
            formGroup.get('from')?.setErrors({ required: true });
            hasErrors = true;
          }
          if (!toValue || toValue <= 0) {
            formGroup.get('to')?.setErrors({ required: true });
            hasErrors = true;
          }
          if (!discountValue && discountValue !== 0) {
            formGroup.get('discount')?.setErrors({ required: true });
            hasErrors = true;
          }

          // Validate range only if both from and to are provided
          if (fromValue > 0 && toValue > 0 && toValue <= fromValue) {
            return { invalidRange: true };
          }

          if (hasErrors) {
            return { incomplete: true };
          }
        }
        return null;
      }
    });

    this.tieredDiscountsFormArray.push(tieredDiscountForm);
  }

  removeTieredDiscount(index: number): void {
    this.tieredDiscountsFormArray.removeAt(index);
  }

  loadCity(): void {
    if (!this.cityId) return;

    this.isLoading = true;
    this.cityClient.getById(this.cityId).subscribe({
      next: (city: CityDto) => {
        this.cityForm.patchValue({
          name: city.name,
          description: city.description ?? null,
          deliveryFees: city.deliveryFees ?? 0,
          urgentDelivery: city.urgentDelivery ?? 0,
          serviceFees: city.serviceFees ?? 0,
          cancellationFees: city.cancellationFees ?? 0
        });

        // Clear existing tiered discounts
        while (this.tieredDiscountsFormArray.length !== 0) {
          this.tieredDiscountsFormArray.removeAt(0);
        }

        // Load tiered discounts if they exist
        if (city.tieredDiscounts && city.tieredDiscounts.length > 0) {
          city.tieredDiscounts.forEach(td => {
            this.addTieredDiscount(td.from, td.to, td.discount, td.id);
          });
        }

        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load city. Please try again.';
        this.isLoading = false;
        console.error('Error loading city:', error);
      }
    });
  }

  onSubmit(): void {
    // Validate main form fields first
    if (this.cityForm.get('name')?.invalid) {
      this.cityForm.markAllAsTouched();
      return;
    }

    // Validate tiered discounts - check if any tier has partial data
    const invalidTiers: number[] = [];
    this.tieredDiscountsFormArray.controls.forEach((control, index) => {
      const from = control.get('from')?.value ?? 0;
      const to = control.get('to')?.value ?? 0;
      const discount = control.get('discount')?.value ?? 0;

      // Check if tier has any data
      const hasAnyData = (from && from > 0) || (to && to > 0) || (discount && discount > 0);

      if (hasAnyData) {
        // If tier has any data, validate it
        control.markAllAsTouched();
        control.updateValueAndValidity();

        if (control.invalid) {
          invalidTiers.push(index + 1);
        }
      }
    });

    // If there are invalid tiers, show error
    if (invalidTiers.length > 0) {
      this.errorMessage = `Please complete tier ${invalidTiers.join(', ')} or remove it.`;
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    const formValue = this.cityForm.value;

    // Map tiered discounts - only include complete and valid ones
    const tieredDiscounts: TieredDiscountDto[] = [];
    this.tieredDiscountsFormArray.controls.forEach(control => {
      const from = control.get('from')?.value ?? 0;
      const to = control.get('to')?.value ?? 0;
      const discount = control.get('discount')?.value ?? 0;

      // Only add if all values are provided, valid, and complete
      if (from && from > 0 && to && to > 0 && discount !== null && discount !== undefined && to > from && discount >= 0 && discount <= 100) {
        const td = new TieredDiscountDto();
        // Set id to 0 for all tiers (backend removes all and recreates them on update)
        td.id = 0;
        // Set cityId: for new cities use 0 (backend will set it), for existing cities use the cityId
        td.cityId = this.isEditMode && this.cityId ? this.cityId : 0;
        td.from = from;
        td.to = to;
        td.discount = discount;
        tieredDiscounts.push(td);
      }
    });

    if (this.isEditMode && this.cityId) {
      const command = new UpdateCityCommand();
      command.cityId = this.cityId;
      command.name = formValue.name;
      command.description = formValue.description || null;
      command.deliveryFees = formValue.deliveryFees ?? null;
      command.urgentDelivery = formValue.urgentDelivery ?? null;
      command.serviceFees = formValue.serviceFees ?? null;
      command.cancellationFees = formValue.cancellationFees ?? null;
      command.tieredDiscounts = tieredDiscounts.length > 0 ? tieredDiscounts : null;

      this.cityClient.update(command).subscribe({
        next: () => {
          this.router.navigate(['/main/cities']);
        },
        error: (error: any) => {
          // Extract error message from backend - check errorMessage first (ProblemDetail structure)
          let errorMessage = 'Failed to update city. Please try again.';
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
          this.errorMessage = errorMessage;
          this.isSaving = false;
          console.error('Error updating city:', error);
        }
      });
    } else {
      const command = new AddCityCommand();
      command.name = formValue.name;
      command.description = formValue.description || null;
      command.deliveryFees = formValue.deliveryFees ?? null;
      command.urgentDelivery = formValue.urgentDelivery ?? null;
      command.serviceFees = formValue.serviceFees ?? null;
      command.cancellationFees = formValue.cancellationFees ?? null;
      command.tieredDiscounts = tieredDiscounts.length > 0 ? tieredDiscounts : null;

      this.cityClient.add(command).subscribe({
        next: () => {
          this.router.navigate(['/main/cities']);
        },
        error: (error: any) => {
          // Extract error message from backend - check errorMessage first (ProblemDetail structure)
          let errorMessage = 'Failed to add city. Please try again.';
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
          this.errorMessage = errorMessage;
          this.isSaving = false;
          console.error('Error adding city:', error);
        }
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/main/cities']);
  }
}

