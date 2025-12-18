import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { AdminLoginCommand } from '../../core/services/clientAPI';
import { ButtonComponent } from '../../shared/components/button/button.component';

@Component({
  selector: 'app-admin-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './admin-login.component.html',
  styleUrl: './admin-login.component.css'
})
export class AdminLoginComponent implements OnInit {
  loginForm!: FormGroup;
  isLoading = false;
  errorMessage = '';

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    // Redirect if already authenticated
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/main']);
      return;
    }

    this.initializeForm();
  }

  private initializeForm(): void {
    this.loginForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(3)]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.markFormGroupTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    const credentials = new AdminLoginCommand();
    credentials.userName = this.loginForm.value.userName;
    credentials.password = this.loginForm.value.password;

    this.authService.login(credentials).subscribe({
      next: (response) => {
        this.isLoading = false;
        // Redirect to main page after successful login
        this.router.navigate(['/main']);
      },
      error: (error) => {
        this.isLoading = false;
        console.error('Login error details:', error);
        
        // Handle different error types
        if (error?.result?.errorMessage) {
          this.errorMessage = error.result.errorMessage;
        } else if (error?.message) {
          this.errorMessage = error.message;
        } else if (error?.status === 0) {
          this.errorMessage = 'Unable to connect to server. Please check if the backend is running.';
        } else if (error?.status === 400) {
          this.errorMessage = 'Invalid username or password. Please try again.';
        } else if (error?.status === 500) {
          this.errorMessage = 'Server error occurred. Please try again later.';
        } else {
          this.errorMessage = 'An unexpected error occurred. Please try again.';
        }
      }
    });
  }

  private markFormGroupTouched(): void {
    Object.keys(this.loginForm.controls).forEach(key => {
      const control = this.loginForm.get(key);
      control?.markAsTouched();
    });
  }

  get userName() {
    return this.loginForm.get('userName');
  }

  get password() {
    return this.loginForm.get('password');
  }

  get userNameError(): string {
    if (this.userName?.hasError('required')) {
      return 'Username is required';
    }
    if (this.userName?.hasError('minlength')) {
      return 'Username must be at least 3 characters';
    }
    return '';
  }

  get passwordError(): string {
    if (this.password?.hasError('required')) {
      return 'Password is required';
    }
    if (this.password?.hasError('minlength')) {
      return 'Password must be at least 6 characters';
    }
    return '';
  }
}

