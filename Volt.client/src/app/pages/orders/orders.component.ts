import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, switchMap, takeUntil } from 'rxjs';
import { AdminOrderClient, OrderDto, PagedResultOfOrderDto, OrderState, PaymentMethod, CityClient, CityDto, PagedResultOfCityDto } from '../../core/services/clientAPI';

@Component({
  selector: 'app-orders',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './orders.component.html',
  styleUrl: './orders.component.css'
})
export class OrdersComponent implements OnInit, OnDestroy {
  orders: OrderDto[] = [];
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  totalPages = 0;
  searchOrderCode = '';
  selectedState: OrderState | null = null;
  isLoading = false;
  errorMessage = '';
  successMessage = '';

  searchFilters = {
    state: null as OrderState | null,
    cityId: null as number | null
  };

  private searchSubject = new Subject<string>();
  private destroy$ = new Subject<void>();

  cities: CityDto[] = [];
  isLoadingCities = false;

  orderStates = [
    { value: null, label: 'All States' },
    { value: OrderState.Pending, label: 'Pending' },
    { value: OrderState.Confirmed, label: 'Confirmed' },
    { value: OrderState.OnWay, label: 'On Way' },
    { value: OrderState.CustomerReceived, label: 'Customer Received' },
    { value: OrderState.Completed, label: 'Completed' }
  ];

  constructor(
    private orderClient: AdminOrderClient,
    private cityClient: CityClient,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadCities();
    this.setupLiveSearch();
    // Initial load
    this.triggerSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
    this.searchSubject.complete();
  }

  setupLiveSearch(): void {
    this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap((orderCode) => {
        this.isLoading = true;
        this.errorMessage = '';
        this.successMessage = '';

        const state = this.searchFilters.state !== null ? this.searchFilters.state :
                      (this.selectedState !== null ? this.selectedState : undefined);
        const cityId = this.searchFilters.cityId !== null && this.searchFilters.cityId > 0 ? this.searchFilters.cityId : undefined;

        return this.orderClient.getAllOrders(
          this.currentPage,
          this.pageSize,
          state,
          orderCode || undefined
        );
      }),
      takeUntil(this.destroy$)
    ).subscribe({
      next: (result: PagedResultOfOrderDto) => {
        let filteredOrders = result.items || [];
        
        // Filter by city on frontend (backend doesn't support city filter yet)
        if (this.searchFilters.cityId !== null && this.searchFilters.cityId > 0) {
          filteredOrders = filteredOrders.filter(order => order.cityId === this.searchFilters.cityId);
        }
        
        this.orders = filteredOrders;
        this.totalCount = filteredOrders.length;
        this.totalPages = Math.ceil(filteredOrders.length / this.pageSize);
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load orders. Please try again.';
        this.isLoading = false;
        console.error('Error loading orders:', error);
      }
    });
  }

  loadCities(): void {
    this.isLoadingCities = true;
    this.cityClient.getAll(1, 1000, undefined, true).subscribe({
      next: (result: PagedResultOfCityDto) => {
        this.cities = result.items || [];
        this.isLoadingCities = false;
      },
      error: (error: any) => {
        console.error('Error loading cities:', error);
        this.isLoadingCities = false;
      }
    });
  }

  loadOrders(): void {
    this.triggerSearch();
  }

  triggerSearch(): void {
    this.searchSubject.next(this.searchOrderCode);
  }

  onSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onStateFilter(state: OrderState | null): void {
    this.selectedState = state;
    this.currentPage = 1;
    this.triggerSearch();
  }

  onAdvancedSearch(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onSearchTermChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.triggerSearch();
  }

  onClearSearch(): void {
    this.searchFilters = {
      state: null,
      cityId: null
    };
    this.searchOrderCode = '';
    this.selectedState = null;
    this.currentPage = 1;
    this.loadOrders();
  }

  hasActiveFilters(): boolean {
    return this.searchFilters.state !== null ||
           (this.searchFilters.cityId !== null && this.searchFilters.cityId > 0) ||
           (this.searchOrderCode && this.searchOrderCode.trim().length > 0) ||
           this.selectedState !== null;
  }

  onView(orderId: number): void {
    this.router.navigate(['/main/orders', orderId]);
  }

  onPageChange(page: number): void {
    if (page >= 1 && page <= this.totalPages) {
      this.currentPage = page;
      this.triggerSearch();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  }

  getPageNumbers(): number[] {
    const pages: number[] = [];
    const maxPages = 5;
    let startPage = Math.max(1, this.currentPage - Math.floor(maxPages / 2));
    let endPage = Math.min(this.totalPages, startPage + maxPages - 1);

    if (endPage - startPage < maxPages - 1) {
      startPage = Math.max(1, endPage - maxPages + 1);
    }

    for (let i = startPage; i <= endPage; i++) {
      pages.push(i);
    }
    return pages;
  }

  getStateLabel(state: OrderState): string {
    switch (state) {
      case OrderState.Pending:
        return 'Pending';
      case OrderState.Confirmed:
        return 'Confirmed';
      case OrderState.OnWay:
        return 'On Way';
      case OrderState.CustomerReceived:
        return 'Customer Received';
      case OrderState.Completed:
        return 'Completed';
      default:
        return 'Unknown';
    }
  }

  getStateClass(state: OrderState): string {
    switch (state) {
      case OrderState.Pending:
        return 'orders__state--pending';
      case OrderState.Confirmed:
        return 'orders__state--confirmed';
      case OrderState.OnWay:
        return 'orders__state--onway';
      case OrderState.CustomerReceived:
        return 'orders__state--received';
      case OrderState.Completed:
        return 'orders__state--completed';
      default:
        return '';
    }
  }

  getPaymentMethodLabel(method: PaymentMethod): string {
    switch (method) {
      case PaymentMethod.Cash:
        return 'Cash';
      case PaymentMethod.PayPal:
        return 'PayPal';
      default:
        return 'Unknown';
    }
  }

  getPaymentMethodClass(method: PaymentMethod): string {
    switch (method) {
      case PaymentMethod.Cash:
        return 'orders__payment--cash';
      case PaymentMethod.PayPal:
        return 'orders__payment--paypal';
      default:
        return '';
    }
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

  get pendingOrdersCount(): number {
    return this.orders.filter(o => o.orderState === OrderState.Pending).length;
  }

  get confirmedOrdersCount(): number {
    return this.orders.filter(o => o.orderState === OrderState.Confirmed).length;
  }

  get onWayOrdersCount(): number {
    return this.orders.filter(o => o.orderState === OrderState.OnWay).length;
  }

  get completedOrdersCount(): number {
    return this.orders.filter(o => o.orderState === OrderState.Completed).length;
  }
}
