import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpRequest, HttpHandler, HttpEvent } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from '../services/auth.service';

@Injectable()
export class AuthInterceptor implements HttpInterceptor {
  constructor(private authService: AuthService) {}

  intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
    // Don't add token to login/register/activate endpoints
    const isAuthEndpoint = req.url.includes('/Login') || 
                          req.url.includes('/Register') || 
                          req.url.includes('/Activate') ||
                          req.url.includes('/RefreshToken');

    if (isAuthEndpoint) {
      return next.handle(req);
    }

    const token = this.authService.getToken();

    if (token) {
      // Clone the request and add the authorization header
      const cloned = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
      return next.handle(cloned);
    }

    return next.handle(req);
  }
}

