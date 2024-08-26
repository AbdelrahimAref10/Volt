import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';


import { importProvidersFrom } from '@angular/core';
import { AppComponent } from './app/app.component';
import { withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { appRouterProviders } from './app/app.routes';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideToastr } from 'ngx-toastr';


bootstrapApplication(AppComponent, {
    providers: [
        appRouterProviders,
        provideHttpClient(withInterceptorsFromDi()), provideAnimationsAsync(),
        provideAnimationsAsync(), // required animations providers
        provideToastr(),
    ]
})
  .catch(err => console.error(err));
