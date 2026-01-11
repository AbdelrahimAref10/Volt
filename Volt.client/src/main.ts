import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { importProvidersFrom } from '@angular/core';
import { AppComponent } from './app/app.component';
import { withInterceptorsFromDi, provideHttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { appRouterProviders } from './app/app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideToastr } from 'ngx-toastr';
import { API_BASE_URL } from './app/core/services/clientAPI';
import { AuthInterceptor } from './app/core/interceptors/auth.interceptor';
import { ErrorInterceptor } from './app/core/interceptors/error.interceptor';

bootstrapApplication(AppComponent, {
    providers: [
        appRouterProviders,
        provideHttpClient(withInterceptorsFromDi()),
        provideAnimationsAsync(),
        provideToastr(),
        // Provide API Base URL - Use empty string to use relative URLs with proxy
        {
            provide: API_BASE_URL,
            useValue: '' // Empty string uses relative URLs, proxy will forward to backend
        },
        // Provide Auth Interceptor
        {
            provide: HTTP_INTERCEPTORS,
            useClass: AuthInterceptor,
            multi: true
        },
        // Provide Error Interceptor (must be after AuthInterceptor to catch 401 errors)
        {
            provide: HTTP_INTERCEPTORS,
            useClass: ErrorInterceptor,
            multi: true
        }
    ]
})
  .catch(err => console.error(err));
