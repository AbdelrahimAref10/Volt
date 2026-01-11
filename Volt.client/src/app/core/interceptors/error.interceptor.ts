import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent, HttpErrorResponse } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, switchMap } from 'rxjs/operators';
import { AuthService } from '../services/auth.service';

@Injectable()
export class ErrorInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    return next.handle(req).pipe(
      catchError((error: HttpErrorResponse) => {
        // Check if error is 401 Unauthorized
        if (error.status === 401) {
          // Don't try to refresh token for auth endpoints
          const isAuthEndpoint = req.url.includes('/Login') || 
                                req.url.includes('/Register') || 
                                req.url.includes('/Activate') ||
                                req.url.includes('/RefreshToken');

          if (isAuthEndpoint) {
            return throwError(() => error);
          }

          return this.handle401Error(req, next);
        }

        return throwError(() => error);
      })
    );
  }

  private handle401Error(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    const token = this.authService.getToken();
    const refreshToken = this.authService.getRefreshToken();

    if (token && refreshToken) {
      return this.authService.refreshToken().pipe(
        switchMap((newToken: string) => {
          // Clone the request with the new token
          const clonedRequest = request.clone({
            setHeaders: {
              Authorization: `Bearer ${newToken}`
            }
          });
          return next.handle(clonedRequest);
        }),
        catchError((err) => {
          // If refresh fails, logout will be handled by AuthService
          return throwError(() => err);
        })
      );
    } else {
      this.authService.logout();
      return throwError(() => new Error('No refresh token available'));
    }
  }
}

