import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { RoleClient, RoleDto, CreateRoleCommand, UpdateRoleCommand } from '../../core/services/clientAPI';

@Component({
  selector: 'app-roles',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './roles.component.html',
  styleUrl: './roles.component.css'
})
export class RolesComponent implements OnInit {
  roles: RoleDto[] = [];
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  showModal = false;
  roleForm: FormGroup;
  isEditing = false;
  editingRoleId: number | null = null;
  isSubmitting = false;

  constructor(
    private roleClient: RoleClient,
    private fb: FormBuilder
  ) {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.minLength(2), Validators.pattern(/^[A-Za-z][A-Za-z0-9]*$/)]]
    });
  }

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.roleClient.getAllRoles().subscribe({
      next: (roles: RoleDto[]) => {
        this.roles = roles;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load roles. Please try again.';
        this.isLoading = false;
        console.error('Error loading roles:', error);
      }
    });
  }

  onAddNew(): void {
    this.isEditing = false;
    this.editingRoleId = null;
    this.roleForm.reset();
    this.showModal = true;
  }

  onEdit(role: RoleDto): void {
    this.isEditing = true;
    this.editingRoleId = role.roleId;
    this.roleForm.patchValue({
      name: role.roleName || ''
    });
    this.showModal = true;
  }

  onDelete(roleId: number, roleName: string): void {
    if (!confirm(`Are you sure you want to delete the role "${roleName}"? This action cannot be undone.`)) {
      return;
    }

    this.roleClient.deleteRole(roleId).subscribe({
      next: () => {
        this.showSuccessMessage('Role deleted successfully');
        this.loadRoles();
      },
      error: (error: any) => {
        const errorMessage = error.error?.detail || error.error?.title || 'Failed to delete role. Please try again.';
        this.showErrorMessage(errorMessage);
        console.error('Error deleting role:', error);
      }
    });
  }

  onSubmit(): void {
    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;
    const formValue = this.roleForm.value;

    if (this.isEditing && this.editingRoleId) {
      // Update role
      const command = new UpdateRoleCommand();
      command.roleId = this.editingRoleId;
      command.roleName = formValue.name;

      this.roleClient.updateRole(command).subscribe({
        next: () => {
          this.showModal = false;
          this.showSuccessMessage('Role updated successfully');
          this.loadRoles();
          this.isSubmitting = false;
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to update role. Please try again.';
          this.showErrorMessage(errorMessage);
          this.isSubmitting = false;
          console.error('Error updating role:', error);
        }
      });
    } else {
      // Create role
      const command = new CreateRoleCommand();
      command.roleName = formValue.name;

      this.roleClient.createRole(command).subscribe({
        next: () => {
          this.showModal = false;
          this.showSuccessMessage('Role created successfully');
          this.loadRoles();
          this.isSubmitting = false;
        },
        error: (error: any) => {
          const errorMessage = error.error?.detail || error.error?.title || 'Failed to create role. Please try again.';
          this.showErrorMessage(errorMessage);
          this.isSubmitting = false;
          console.error('Error creating role:', error);
        }
      });
    }
  }

  onCloseModal(): void {
    this.showModal = false;
    this.roleForm.reset();
    this.isEditing = false;
    this.editingRoleId = null;
    this.isSubmitting = false;
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
}

