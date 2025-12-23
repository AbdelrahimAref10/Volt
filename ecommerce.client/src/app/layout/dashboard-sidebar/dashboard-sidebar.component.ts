import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

interface MenuItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-dashboard-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-sidebar.component.html',
  styleUrl: './dashboard-sidebar.component.css'
})
export class DashboardSidebarComponent {
  @Input() isOpen: boolean = true;

  menuItems: MenuItem[] = [
    { label: 'Dashboard', route: '/main/dashboard', icon: 'dashboard' },
    { label: 'Categories', route: '/main/categories', icon: 'categories' },
    { label: 'SubCategories', route: '/main/subcategories', icon: 'subcategories' },
    { label: 'Vehicles', route: '/main/vehicles', icon: 'vehicles' },
    { label: 'Customers', route: '/main/customers', icon: 'users' },
    { label: 'Cities', route: '/main/cities', icon: 'cities' },
    { label: 'Orders', route: '/main/orders', icon: 'orders' },
    { label: 'Reports', route: '/main/reports', icon: 'reports' },
    { label: 'Support', route: '/main/support', icon: 'support' },
  ];
}


