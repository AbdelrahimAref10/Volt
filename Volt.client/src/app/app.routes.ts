import { Routes, provideRouter } from '@angular/router';
import { DashboardLayoutComponent } from './layout/dashboard-layout/dashboard-layout.component';
import { AdminLoginComponent } from './pages/admin-login/admin-login.component';
import { AuthGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'login', component: AdminLoginComponent },
  {
    path: 'main',
    component: DashboardLayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () => import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'categories',
        loadComponent: () => import('./pages/categories/categories.component').then(m => m.CategoriesComponent)
      },
      {
        path: 'categories/new',
        loadComponent: () => import('./pages/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'categories/:id/edit',
        loadComponent: () => import('./pages/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'subcategories',
        loadComponent: () => import('./pages/subcategories/subcategories.component').then(m => m.SubCategoriesComponent)
      },
      {
        path: 'subcategories/new',
        loadComponent: () => import('./pages/subcategories/subcategory-form/subcategory-form.component').then(m => m.SubCategoryFormComponent)
      },
      {
        path: 'subcategories/:id/edit',
        loadComponent: () => import('./pages/subcategories/subcategory-form/subcategory-form.component').then(m => m.SubCategoryFormComponent)
      },
      {
        path: 'vehicles',
        loadComponent: () => import('./pages/vehicles/vehicles.component').then(m => m.VehiclesComponent)
      },
      {
        path: 'vehicles/new',
        loadComponent: () => import('./pages/vehicles/vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent)
      },
      {
        path: 'vehicles/:id/edit',
        loadComponent: () => import('./pages/vehicles/vehicle-form/vehicle-form.component').then(m => m.VehicleFormComponent)
      },
      {
        path: 'customers',
        loadComponent: () => import('./pages/customers/customers.component').then(m => m.CustomersComponent)
      },
      {
        path: 'customers/new',
        loadComponent: () => import('./pages/customers/customer-form/customer-form.component').then(m => m.CustomerFormComponent)
      },
      {
        path: 'customers/:id',
        loadComponent: () => import('./pages/customers/customer-detail/customer-detail.component').then(m => m.CustomerDetailComponent)
      },
      {
        path: 'users',
        loadComponent: () => import('./pages/users/users.component').then(m => m.UsersComponent)
      },
      {
        path: 'users/:id',
        loadComponent: () => import('./pages/users/user-detail/user-detail.component').then(m => m.UserDetailComponent)
      },
      {
        path: 'roles',
        loadComponent: () => import('./pages/roles/roles.component').then(m => m.RolesComponent)
      },
      {
        path: 'cities',
        loadComponent: () => import('./pages/cities/cities.component').then(m => m.CitiesComponent)
      },
      {
        path: 'cities/new',
        loadComponent: () => import('./pages/cities/city-form/city-form.component').then(m => m.CityFormComponent)
      },
      {
        path: 'cities/:id/edit',
        loadComponent: () => import('./pages/cities/city-form/city-form.component').then(m => m.CityFormComponent)
      },
      {
        path: 'orders',
        loadComponent: () => import('./pages/orders/orders.component').then(m => m.OrdersComponent)
      },
      {
        path: 'orders/:id',
        loadComponent: () => import('./pages/orders/order-detail/order-detail.component').then(m => m.OrderDetailComponent)
      },
      {
        path: 'reports',
        loadComponent: () => import('./pages/reports/reports.component').then(m => m.ReportsComponent)
      },
      {
        path: 'reports/orders-by-state',
        loadComponent: () => import('./pages/reports/orders-by-state-report/orders-by-state-report.component').then(m => m.OrdersByStateReportComponent)
      },
      {
        path: 'reports/orders-by-date-range',
        loadComponent: () => import('./pages/reports/orders-by-date-range-report/orders-by-date-range-report.component').then(m => m.OrdersByDateRangeReportComponent)
      },
      {
        path: 'reports/revenue',
        loadComponent: () => import('./pages/reports/revenue-report/revenue-report.component').then(m => m.RevenueReportComponent)
      },
      {
        path: 'reports/revenue-by-period',
        loadComponent: () => import('./pages/reports/revenue-by-period-report/revenue-by-period-report.component').then(m => m.RevenueByPeriodReportComponent)
      },
      {
        path: 'reports/cancellations',
        loadComponent: () => import('./pages/reports/cancellations-report/cancellations-report.component').then(m => m.CancellationsReportComponent)
      },
      {
        path: 'reports/cancellation-fees',
        loadComponent: () => import('./pages/reports/cancellation-fees-report/cancellation-fees-report.component').then(m => m.CancellationFeesReportComponent)
      },
      {
        path: 'reports/vehicle-utilization',
        loadComponent: () => import('./pages/reports/vehicle-utilization-report/vehicle-utilization-report.component').then(m => m.VehicleUtilizationReportComponent)
      },
      {
        path: 'reports/customer-order-history',
        loadComponent: () => import('./pages/reports/customer-order-history-report/customer-order-history-report.component').then(m => m.CustomerOrderHistoryReportComponent)
      },
      {
        path: 'reports/treasury-balance',
        loadComponent: () => import('./pages/reports/treasury-balance-report/treasury-balance-report.component').then(m => m.TreasuryBalanceReportComponent)
      },
      {
        path: 'reports/wallet',
        loadComponent: () => import('./pages/reports/wallet-report/wallet-report.component').then(m => m.WalletReportComponent)
      },
      {
        path: 'support',
        loadComponent: () => import('./pages/support/support.component').then(m => m.SupportComponent)
      },
      {
        path: 'profile',
        loadComponent: () => import('./pages/profile/profile.component').then(m => m.ProfileComponent)
      },
    ]
  },
  { path: '**', redirectTo: '/login' }
];

export const appRouterProviders = [provideRouter(routes)];
