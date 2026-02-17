import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AdminUserClient, UserDto, UpdateUserCommand, RoleClient, RoleDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css'
})
export class UserDetailComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private adminUserClient = inject(AdminUserClient);
  private roleClient = inject(RoleClient);
  private fb = inject(FormBuilder);

  user: UserDto | null = null;
  userId: number = 0;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  isEditing = false;
  actionLoading: string = '';

  userForm: FormGroup;
  availableRoles: RoleDto[] = [];
  isLoadingRoles = false;
  showPassword: boolean = false;

  // Multi-step form
  currentStep: number = 1;
  totalSteps: number = 3;
  steps = [
    { number: 1, title: 'Basic Information', fields: ['userName', 'email', 'phoneNumber'] },
    { number: 2, title: 'Security', fields: ['password'] },
    { number: 3, title: 'Role Assignment', fields: ['role'] }
  ];

  constructor() {
    this.userForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      phoneNumber: ['', [Validators.required]],
      password: [''],
      role: [null, [Validators.required]]
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.userId = +params['id'];
      if (this.userId) {
        this.loadUser();
        this.loadRoles();
      }
    });
  }

  loadUser(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.adminUserClient.getById(this.userId).subscribe({
      next: (user: UserDto) => {
        this.user = user;
        this.userForm.patchValue({
          userName: user.userName,
          email: user.email || '',
          phoneNumber: user.phoneNumber || '',
          password: '',
          role: user.roles && user.roles.length > 0 ? user.roles[0] : null
        });
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load user details.';
        this.isLoading = false;
        console.error('Error loading user:', error);
      }
    });
  }

  loadRoles(): void {
    this.isLoadingRoles = true;
    this.roleClient.getAllRoles().subscribe({
      next: (roles: RoleDto[]) => {
        this.availableRoles = roles.filter(r => r.roleName && r.roleName.length > 0);
        this.isLoadingRoles = false;
      },
      error: (error: any) => {
        console.error('Error loading roles:', error);
        this.isLoadingRoles = false;
      }
    });
  }

  getRoleIdByName(roleName: string): number | null {
    const role = this.availableRoles.find(r => r.roleName === roleName);
    return role ? role.roleId : null;
  }

  onEdit(): void {
    this.isEditing = true;
    this.currentStep = 1;
  }

  // Multi-step form methods
  getFormProgress(): number {
    const totalFields = this.steps.reduce((sum, step) => sum + step.fields.length, 0);
    let completedFields = 0;

    this.steps.forEach(step => {
      step.fields.forEach(field => {
        const control = this.userForm.get(field);
        if (control && control.valid && (control.value || field === 'password')) {
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
      if (control && control.valid && (control.value || field === 'password')) {
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
      // Password is optional for update
      if (field === 'password') return true;
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
      if (stepNumber < this.currentStep || this.isStepValid(this.currentStep)) {
        this.currentStep = stepNumber;
      }
    }
  }

  togglePasswordVisibility(): void {
    this.showPassword = !this.showPassword;
  }

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

  isStep1(): boolean {
    return this.currentStep === 1;
  }

  isStep2(): boolean {
    return this.currentStep === 2;
  }

  isStep3(): boolean {
    return this.currentStep === 3;
  }

  onCancel(): void {
    this.isEditing = false;
    this.currentStep = 1;
    if (this.user) {
      this.userForm.patchValue({
        userName: this.user.userName,
        email: this.user.email || '',
        phoneNumber: this.user.phoneNumber || '',
        password: '',
        role: this.user.roles && this.user.roles.length > 0 ? this.user.roles[0] : null
      });
    }
  }

  onSave(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }

    this.actionLoading = 'save';
    const formValue = this.userForm.value;
    const command = new UpdateUserCommand();
    command.userId = this.userId;
    command.userName = formValue.userName;
    command.email = formValue.email || null;
    command.phoneNumber = formValue.phoneNumber || null;
    command.password = formValue.password && formValue.password.trim() !== '' ? formValue.password : null;

    // Get roleId from role name
    const roleId = this.getRoleIdByName(formValue.role);
    if (!roleId) {
      this.showErrorMessage('Please select a valid role');
      this.actionLoading = '';
      return;
    }
    command.roleId = roleId;

    this.adminUserClient.update(this.userId, command).subscribe({
      next: () => {
        this.showSuccessMessage('User updated successfully');
        this.isEditing = false;
        this.loadUser();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to update user');
        this.actionLoading = '';
      }
    });
  }

  onDelete(): void {
    if (!confirm('Are you sure you want to delete this user? This action cannot be undone.')) {
      return;
    }

    this.actionLoading = 'delete';
    this.adminUserClient.delete(this.userId).subscribe({
      next: () => {
        this.showSuccessMessage('User deleted successfully');
        setTimeout(() => {
          this.router.navigate(['/main/users']);
        }, 1500);
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to delete user');
        this.actionLoading = '';
      }
    });
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

  onBack(): void {
    this.router.navigate(['/main/users']);
  }

  isLockedOut(): boolean {
    if (!this.user || !this.user.lockoutEnd) return false;
    return new Date(this.user.lockoutEnd) > new Date();
  }

  getLockoutStatus(): string {
    if (!this.user) return 'Unknown';
    return this.user.isActive ? 'Active' : 'Not Active';
  }

  onActivate(): void {
    if (!confirm('Are you sure you want to activate this user?')) {
      return;
    }

    this.actionLoading = 'activate';
    this.adminUserClient.activate(this.userId).subscribe({
      next: () => {
        this.showSuccessMessage('User activated successfully');
        this.loadUser();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to activate user');
        this.actionLoading = '';
      }
    });
  }

  onDeactivate(): void {
    if (!confirm('Are you sure you want to deactivate this user?')) {
      return;
    }

    this.actionLoading = 'deactivate';
    this.adminUserClient.deactivate(this.userId).subscribe({
      next: () => {
        this.showSuccessMessage('User deactivated successfully');
        this.loadUser();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.detail || error.error?.title || 'Failed to deactivate user');
        this.actionLoading = '';
      }
    });
  }
}

