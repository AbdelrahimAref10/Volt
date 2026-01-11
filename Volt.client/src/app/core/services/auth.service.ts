import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { AdminClient, AdminLoginCommand, AdminLoginResponse, CustomerClient, RefreshTokenCommand, RefreshTokenResponse } from './clientAPI';
import { Observable, tap, catchError, throwError, BehaviorSubject, map, filter, take } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly REFRESH_TOKEN_KEY = 'refresh_token';
  private readonly USER_KEY = 'user_data';
  private refreshTokenInProgress = false;
  private refreshTokenSubject = new BehaviorSubject<string | null>(null);

  constructor(
    private adminClient: AdminClient,
    private customerClient: CustomerClient,
    private router: Router
  ) {}

  login(credentials: AdminLoginCommand): Observable<AdminLoginResponse> {
    return this.adminClient.login(credentials).pipe(
      tap((response: AdminLoginResponse) => {
        // Store tokens and user data
        this.setToken(response.token);
        this.setRefreshToken(response.refreshToken);
        this.setUserData({
          userId: response.userId,
          userName: response.userName,
          roles: response.roles
        });
      }),
      catchError((error) => {
        console.error('Login error:', error);
        return throwError(() => error);
      })
    );
  }

  logout(): void {
    this.clearAuthData();
    this.router.navigate(['/login']);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getToken(): string | null {
    return localStorage.getItem(this.TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.REFRESH_TOKEN_KEY);
  }

  getUserData(): any {
    const userData = localStorage.getItem(this.USER_KEY);
    return userData ? JSON.parse(userData) : null;
  }

  private setToken(token: string): void {
    localStorage.setItem(this.TOKEN_KEY, token);
  }

  private setRefreshToken(refreshToken: string): void {
    localStorage.setItem(this.REFRESH_TOKEN_KEY, refreshToken);
  }

  private setUserData(userData: any): void {
    localStorage.setItem(this.USER_KEY, JSON.stringify(userData));
  }

  private clearAuthData(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.REFRESH_TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
  }

  refreshToken(): Observable<string> {
    const currentToken = this.getToken();
    const refreshTokenValue = this.getRefreshToken();

    if (!currentToken || !refreshTokenValue) {
      this.logout();
      return throwError(() => new Error('No token or refresh token available'));
    }

    // If refresh is already in progress, wait for it to complete
    if (this.refreshTokenInProgress) {
      return this.refreshTokenSubject.asObservable().pipe(
        filter(token => token !== null),
        take(1),
        map(token => token as string)
      );
    }

    this.refreshTokenInProgress = true;
    this.refreshTokenSubject.next(null);

    const command = new RefreshTokenCommand();
    command.token = currentToken;
    command.refreshToken = refreshTokenValue;

    return this.customerClient.refreshToken(command).pipe(
      map((response: RefreshTokenResponse) => {
        this.setToken(response.token);
        this.setRefreshToken(response.refreshToken);
        this.refreshTokenInProgress = false;
        this.refreshTokenSubject.next(response.token);
        return response.token;
      }),
      catchError((error) => {
        this.refreshTokenInProgress = false;
        this.refreshTokenSubject.next(null);
        this.logout();
        return throwError(() => error);
      })
    );
  }
}

