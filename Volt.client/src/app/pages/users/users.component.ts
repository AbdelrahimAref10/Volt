import { Component, OnInit, OnDestroy, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators, AbstractControl, ValidationErrors } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { AdminUserClient, UserDto, PagedResultOfUserDto, CreateUserCommand, RoleClient, RoleDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-users',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './users.component.html',
  styleUrl: './users.component.css'
})
export class UsersComponent implements OnInit, OnDestroy {
  private adminUserClient = inject(AdminUserClient);
  private roleClient = inject(RoleClient);
  private fb = inject(FormBuilder);
  private router = inject(Router);

  users: UserDto[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchTerm = '';
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  searchFilters = {
    role: null as string | null,
    active: null as boolean | null
  };

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  showModal = false;
  userForm: FormGroup;
  roles: RoleDto[] = [];
  availableRoles: string[] = [];
  isLoadingRoles = false;
  isSubmitting = false;
  showPassword = false;
  
  // Multi-step form
  currentStep: number;
  totalSteps: number = 3;
  steps = [
    { number: 1, title: 'Basic Information', fields: ['userName', 'email', 'phoneNumber'] },
    { number: 2, title: 'Security', fields: ['password'] },
    { number: 3, title: 'Role Assignment', fields: ['role'] }
  ];

  constructor() {
    this.currentStep = 1;
    this.userForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      password: ['', [Validators.required, Validators.minLength(8), this.passwordValidator]],
      role: [null, [Validators.required]]
    });
  }

  // Custom password validator for lowercase and uppercase requirements
  passwordValidator(control: AbstractControl): ValidationErrors | null {
    if (!control.value) {
      return null; // Let required validator handle empty values
    }

    const password = control.value;
    const hasLowercase = /[a-z]/.test(password);
    const hasUppercase = /[A-Z]/.test(password);
    const errors: ValidationErrors = {};

    if (!hasLowercase) {
      errors['passwordRequireLower'] = true;
    }
    if (!hasUppercase) {
      errors['passwordRequireUpper'] = true;
    }

    return Object.keys(errors).length > 0 ? errors : null;
  }

  // Get password strength
  getPasswordStrength(): { strength: 'weak' | 'medium' | 'strong', percentage: number } {
    const password = this.userForm.get('password')?.value || '';
    if (!password) return { strength: 'weak', percentage: 0 };

    let strength = 0;
    if (password.length >= 8) strength += 25;
    if (password.length >= 12) strength += 10;
    if (/[a-z]/.test(password)) strength += 20;
    if (/[A-Z]/.test(password)) strength += 20;
    if (/[0-9]/.test(password)) strength += 15;
    if (/[^a-zA-Z0-9]/.test(password)) strength += 10;

    if (strength < 50) return { strength: 'weak', percentage: strength };
    if (strength < 80) return { strength: 'medium', percentage: strength };
    return { strength: 'strong', percentage: strength };
  }

  // Check if field has error
  hasFieldError(fieldName: string): boolean {
    const field = this.userForm.get(fieldName);
    return !!(field && field.invalid && (field.dirty || field.touched));
  }

  // Get field error message
  getFieldError(fieldName: string): string {
    const field = this.userForm.get(fieldName);
    if (!field || !field.errors) return '';

    if (field.errors['required']) {
      return `${this.getFieldLabel(fieldName)} is required`;
    }
    if (field.errors['minlength']) {
      const requiredLength = field.errors['minlength'].requiredLength;
      return `${this.getFieldLabel(fieldName)} must be at least ${requiredLength} characters`;
    }
    if (field.errors['email']) {
      return 'Please enter a valid email address';
    }
    if (field.errors['passwordRequireLower']) {
      return 'Password must contain at least one lowercase letter';
    }
    if (field.errors['passwordRequireUpper']) {
      return 'Password must contain at least one uppercase letter';
    }
    return '';
  }

  // Get field label
  getFieldLabel(fieldName: string): string {
    const labels: { [key: string]: string } = {
      'userName': 'Username',
      'email': 'Email',
      'phoneNumber': 'Phone Number',
      'password': 'Password',
      'role': 'Role'
    };
    return labels[fieldName] || fieldName;
  }

  // Toggle password visibility
  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

  // Password requirement checkers
  hasPasswordMinLength(): boolean {
    const password = this.userForm.get('password')?.value || '';
    return password.length >= 8;
  }

  hasPasswordLowercase(): boolean {
    const password = this.userForm.get('password')?.value || '';
    return /[a-z]/.test(password);
  }

  hasPasswordUppercase(): boolean {
    const password = this.userForm.get('password')?.value || '';
    return /[A-Z]/.test(password);
  }

  ngOnInit(): void {
    this.loadRoles();
    this.setupLiveSearch();
    this.triggerSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.searchSubject.complete();
  }

  setupLiveSearch(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((searchTerm) => {
        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        return this.adminUserClient.getAll(
          this.currentPage,
          this.pageSize,
          searchTerm || undefined,
          this.searchFilters.role || undefined,
          this.searchFilters.active !== null ? this.searchFilters.active : undefined
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (result: PagedResultOfUserDto) => {
        this.users = result.items || [];
        this.totalCount = result.totalCount || 0;
        this.totalPages = result.totalPages || 0;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load users. Please try again.';
        this.isLoading = false;
        console.error('Error loading users:', error);
      }
    });
  }

  triggerSearch(): void {
    this.searchSubject.next(this.searchTerm);
  }

  loadRoles(): void {
    this.isLoadingRoles = true;
    this.roleClient.getAllRoles().subscribe({
      next: (roles: RoleDto[]) => {
        this.roles = roles;
        this.availableRoles = roles.map(r => r.roleName || '').filter(name => name.length > 0);
        this.isLoadingRoles = false;
      },
      error: (error: any) => {
        console.error('Error loading roles:', error);
        this.isLoadingRoles = false;
      }
    });
  }

  getRoleIdByName(roleName: string): number | null {
    const role = this.roles.find(r => r.roleName === roleName);
    return role ? role.roleId : null;
  }

  loadUsers(): void {
    this.triggerSearch();
  }

  onSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onAdvancedSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onSearchTermChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onClearSearch(): void {
    this.searchFilters = {
      role: null,
      active: null
    };
    this.searchTerm = '';
    this.currentPage = 1;
    this.loadUsers();
  }

  hasActiveFilters(): boolean {
    return this.searchFilters.role !== null ||
           this.searchFilters.active !== null ||
           !!(this.searchTerm && this.searchTerm.trim().length > 0);
  }


  onAddNew(): void {
    this.userForm.reset();
    this.userForm.patchValue({
      role: null
    });
    this.currentStep = 1;
    this.showModal = true;
  }

  // Multi-step form methods
  getFormProgress(): number {
    const totalFields = this.steps.reduce((sum, step) => sum + step.fields.length, 0);
    let completedFields = 0;
    
    this.steps.forEach(step => {
      step.fields.forEach(field => {
        const control = this.userForm.get(field);
        if (control && control.valid && control.value) {
          completedFields++;
        }
      });
    });
    
    return Math.round((completedFields / totalFields) * 100);
  }

  getStepProgress(stepNumber: number): number {
    const step = this.steps.find(s => s.number === stepNumber);
    if (!step) return 0;
    
    let completedFields = 0;
    step.fields.forEach(field => {
      const control = this.userForm.get(field);
      if (control && control.valid && control.value) {
        completedFields++;
      }
    });
    
    return Math.round((completedFields / step.fields.length) * 100);
  }

  isStepValid(stepNumber: number): boolean {
    const step = this.steps.find(s => s.number === stepNumber);
    if (!step) return false;
    
    return step.fields.every(field => {
      const control = this.userForm.get(field);
      return control && control.valid;
    });
  }

  canGoToNextStep(): boolean {
    return this.isStepValid(this.currentStep);
  }

  onNextStep(): void {
    if (this.canGoToNextStep() && this.currentStep < this.totalSteps) {
      this.currentStep++;
    }
  }

  onPreviousStep(): void {
    if (this.currentStep > 1) {
      this.currentStep--;
    }
  }

  goToStep(stepNumber: number): void {
    if (stepNumber >= 1 && stepNumber <= this.totalSteps) {
      // Allow going back to previous steps, but validate current step before going forward
      if (stepNumber < this.currentStep || this.isStepValid(this.currentStep)) {
        this.currentStep = stepNumber;
      }
    }
  }

  isStep1(): boolean {
    return this.currentStep === 1;
  }

  isStep2(): boolean {
    return this.currentStep === 2;
  }

  isStep3(): boolean {
    return this.currentStep === 3;
  }

  onView(userId: number): void {
    this.router.navigate(['/main/users', userId]);
  }

  onEdit(userId: number): void {
    this.router.navigate(['/main/users', userId, 'edit']);
  }

  onDelete(userId: number): void {
    // Note: This component already has good error handling, but we should add confirmation dialog
    // For now, keeping the existing confirm but ensuring backend errors are shown
    if (confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      this.adminUserClient.delete(userId).subscribe({
        next: () => {
          this.showSuccessMessage('User deleted successfully');
          this.loadUsers();
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to delete user. Please try again.';
          this.showErrorMessage(errorMessage);
          console.error('Error deleting user:', error);
        }
      });
    }
  }

  onActivate(userId: number): void {
    if (confirm('Are you sure you want to activate this user?')) {
      this.isLoading = true;
      this.adminUserClient.activate(userId).subscribe({
        next: () => {
          this.isLoading = false;
          this.showSuccessMessage('User activated successfully');
          this.loadUsers();
        },
        error: (error: any) => {
          this.isLoading = false;
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to activate user. Please try again.';
          this.showErrorMessage(errorMessage);
          console.error('Error activating user:', error);
        }
      });
    }
  }

  onDeactivate(userId: number): void {
    if (confirm('Are you sure you want to deactivate this user?')) {
      this.isLoading = true;
      this.adminUserClient.deactivate(userId).subscribe({
        next: () => {
          this.isLoading = false;
          this.showSuccessMessage('User deactivated successfully');
          this.loadUsers();
        },
        error: (error: any) => {
          this.isLoading = false;
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to deactivate user. Please try again.';
          this.showErrorMessage(errorMessage);
          console.error('Error deactivating user:', error);
        }
      });
    }
  }


  async onSubmit(): Promise<void> {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.userForm.value;

    const command = new CreateUserCommand();
    command.userName = formValue.userName;
    command.email = formValue.email || null;
    command.phoneNumber = formValue.phoneNumber || null;
    command.password = formValue.password;

    // Get roleId from role name
    const roleId = this.getRoleIdByName(formValue.role);
    if (!roleId) {
      this.showErrorMessage('Please select a valid role');
      this.isSubmitting = false;
      return;
    }
    command.roleId = roleId;

    this.adminUserClient.create(command).subscribe({
      next: () => {
        this.isSubmitting = false;
        this.showModal = false;
        this.userForm.reset();
        this.userForm.patchValue({
          role: null
        });
        this.errorMessage = ''; // Clear any error messages
        this.showSuccessMessage('User created successfully');
        this.loadUsers();
      },
      error: (error: any) => {
        const errorMessage = this.extractErrorMessage(error);
        this.showErrorMessage(errorMessage);
        this.isSubmitting = false;
        console.error('Error creating user:', error);
      }
    });
  }

  onCloseModal(): void {
    this.showModal = false;
    this.userForm.reset();
    this.userForm.patchValue({
      role: null
    });
    this.currentStep = 1;
    this.isSubmitting = false;
    this.errorMessage = ''; // Clear error message when closing modal
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.triggerSearch();
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

  // Extract error message from backend response dynamically
  extractErrorMessage(error: any): string {
    if (!error) {
      return 'An unexpected error occurred. Please try again.';
    }

    // Check for error.error (common in Angular HTTP errors)
    const errorObj = error.error || error;

    // Check for array of errors (common in ASP.NET Core validation)
    if (errorObj.errors && Array.isArray(errorObj.errors)) {
      return errorObj.errors.join(', ');
    }

    // Check for errors object with key-value pairs (ASP.NET Core ModelState)
    if (errorObj.errors && typeof errorObj.errors === 'object') {
      const errorMessages: string[] = [];
      for (const key in errorObj.errors) {
        if (errorObj.errors.hasOwnProperty(key)) {
          const fieldErrors = errorObj.errors[key];
          if (Array.isArray(fieldErrors)) {
            errorMessages.push(...fieldErrors);
          } else if (typeof fieldErrors === 'string') {
            errorMessages.push(fieldErrors);
          }
        }
      }
      if (errorMessages.length > 0) {
        return errorMessages.join(', ');
      }
    }

    // Check for detail property (ProblemDetails standard)
    if (errorObj.detail) {
      return errorObj.detail;
    }

    // Check for title property (ProblemDetails standard)
    if (errorObj.title) {
      return errorObj.title;
    }

    // Check for message property
    if (errorObj.message) {
      return errorObj.message;
    }

    // Check for error message string
    if (typeof errorObj === 'string') {
      return errorObj;
    }

    // Check for statusText
    if (error.statusText) {
      return error.statusText;
    }

    // Fallback to default message
    return 'Failed to create user. Please try again.';
  }

  getRolesDisplay(roles: string[]): string {
    return roles && roles.length > 0 ? roles.join(', ') : 'No roles';
  }

  getActiveUsersCount(): number {
    return this.users.filter(u => u.active).length;
  }

  getInactiveUsersCount(): number {
    return this.users.filter(u => !u.active).length;
  }

  getStatus(user: UserDto): string {
    return user.active ? 'Active' : 'Not Active';
  }
}
