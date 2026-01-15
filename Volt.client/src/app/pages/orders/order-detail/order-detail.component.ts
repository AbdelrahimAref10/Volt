import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminOrderClient, VehicleClient, OrderDetailDto, OrderState, PaymentMethod, PaymentState, UpdateOrderStateCommand, VehicleDto, PagedResultOfVehicleDto } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit {
  order: OrderDetailDto | null = null;
  orderId: number = 0;
  isLoading = false;
  errorMessage = '';
  successMessage = '';
  actionLoading: string = '';

  // Vehicle Assignment
  showVehicleModal = false;
  availableVehicles: VehicleDto[] = [];
  selectedVehicleIds: number[] = [];
  isLoadingVehicles = false;

  // State Management
  showStateModal = false;
  newState: OrderState | null = null;


  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private orderClient: AdminOrderClient,
    private vehicleClient: VehicleClient
  ) {}

  ngOnInit(): void {
    this.route.params.subscribe(params => {
      this.orderId = +params['id'];
      if (this.orderId) {
        this.loadOrder();
      }
    });
  }

  loadOrder(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.orderClient.getOrderById(this.orderId).subscribe({
      next: (order: OrderDetailDto) => {
        this.order = order;
        this.isLoading = false;
      },
      error: (error: any) => {
        this.errorMessage = 'Failed to load order details.';
        this.isLoading = false;
        console.error('Error loading order:', error);
      }
    });
  }

  // Vehicle Assignment
  onAssignVehicles(): void {
    if (!this.order) return;
    
    this.isLoadingVehicles = true;
    this.vehicleClient.getBySubCategory(this.order.subCategoryId, 1, 1000).subscribe({
      next: (result: PagedResultOfVehicleDto) => {
        this.availableVehicles = result.items?.filter((v: VehicleDto) => v.status === 'Available') || [];
        this.selectedVehicleIds = [];
        this.showVehicleModal = true;
        this.isLoadingVehicles = false;
      },
      error: (error: any) => {
        this.showErrorMessage('Failed to load available vehicles.');
        this.isLoadingVehicles = false;
        console.error('Error loading vehicles:', error);
      }
    });
  }

  onConfirmOrder(): void {
    if (!this.order || this.selectedVehicleIds.length !== this.order.vehiclesCount) {
      this.showErrorMessage(`Please select exactly ${this.order?.vehiclesCount || 0} vehicle(s).`);
      return;
    }

    // Clear any previous error messages
    this.errorMessage = '';
    this.successMessage = '';

    if (!confirm(`Are you sure you want to confirm this order and assign ${this.selectedVehicleIds.length} vehicle(s)?`)) {
      return;
    }

    this.actionLoading = 'confirm';
    const command = new UpdateOrderStateCommand();
    command.orderId = this.orderId;
    command.newState = OrderState.Confirmed;
    command.vehicleIds = this.selectedVehicleIds;

    this.orderClient.updateOrderState(this.orderId, command).subscribe({
      next: () => {
        this.showVehicleModal = false;
        this.showSuccessMessage('Order confirmed successfully');
        this.loadOrder();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.errorMessage || 'Failed to confirm order');
        this.actionLoading = '';
      }
    });
  }

  onCloseVehicleModal(): void {
    this.showVehicleModal = false;
    this.selectedVehicleIds = [];
  }

  toggleVehicleSelection(vehicleId: number): void {
    const index = this.selectedVehicleIds.indexOf(vehicleId);
    if (index > -1) {
      this.selectedVehicleIds.splice(index, 1);
    } else {
      if (this.order && this.selectedVehicleIds.length < this.order.vehiclesCount) {
        this.selectedVehicleIds.push(vehicleId);
      } else {
        this.showErrorMessage(`You can only select ${this.order?.vehiclesCount} vehicle(s).`);
      }
    }
  }

  isVehicleSelected(vehicleId: number): boolean {
    return this.selectedVehicleIds.includes(vehicleId);
  }

  // State Management
  onUpdateState(state: OrderState): void {
    if (!this.order) return;

    // Clear any previous error messages
    this.errorMessage = '';
    this.successMessage = '';

    let confirmMessage = '';
    switch (state) {
      case OrderState.Confirmed:
        confirmMessage = 'Confirm this order?';
        break;
      case OrderState.OnWay:
        confirmMessage = 'Mark order as On Way?';
        break;
      case OrderState.CustomerReceived:
        confirmMessage = 'Mark customer as received vehicle?';
        break;
      case OrderState.Completed:
        confirmMessage = 'Complete this order? This will update the treasury.';
        break;
      default:
        return;
    }

    if (!confirm(confirmMessage)) return;

    this.actionLoading = `state-${state}`;
    const command = new UpdateOrderStateCommand();
    command.orderId = this.orderId;
    command.newState = state;

    this.orderClient.updateOrderState(this.orderId, command).subscribe({
      next: () => {
        this.showSuccessMessage('Order state updated successfully');
        this.loadOrder();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.errorMessage || 'Failed to update order state');
        this.actionLoading = '';
      }
    });
  }


  // Cancellation
  onCancelOrder(): void {
    if (!this.order) return;

    // Clear any previous error messages
    this.errorMessage = '';
    this.successMessage = '';

    if (!confirm('Are you sure you want to cancel this order? This action cannot be undone.')) return;

    this.actionLoading = 'cancel';
    this.orderClient.cancelOrder(this.orderId).subscribe({
      next: () => {
        this.showSuccessMessage('Order cancelled successfully');
        this.loadOrder();
        this.actionLoading = '';
      },
      error: (error: any) => {
        this.showErrorMessage(error.error?.errorMessage || 'Failed to cancel order');
        this.actionLoading = '';
      }
    });
  }


  // Navigation
  onBack(): void {
    this.router.navigate(['/main/orders']);
  }

  // Helper Methods
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
        return 'order-detail__state--pending';
      case OrderState.Confirmed:
        return 'order-detail__state--confirmed';
      case OrderState.OnWay:
        return 'order-detail__state--onway';
      case OrderState.CustomerReceived:
        return 'order-detail__state--received';
      case OrderState.Completed:
        return 'order-detail__state--completed';
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

  getPaymentStateLabel(state: PaymentState): string {
    switch (state) {
      case PaymentState.Pending:
        return 'Pending';
      case PaymentState.Paid:
        return 'Paid';
      case PaymentState.Failed:
        return 'Failed';
      case PaymentState.Refunded:
        return 'Refunded';
      default:
        return 'Unknown';
    }
  }

  getPaymentStateClass(state: PaymentState): string {
    switch (state) {
      case PaymentState.Pending:
        return 'order-detail__payment-state--pending';
      case PaymentState.Paid:
        return 'order-detail__payment-state--paid';
      case PaymentState.Failed:
        return 'order-detail__payment-state--failed';
      case PaymentState.Refunded:
        return 'order-detail__payment-state--refunded';
      default:
        return '';
    }
  }

  canConfirm(): boolean {
    return this.order?.orderState === OrderState.Pending;
  }

  canUpdateToOnWay(): boolean {
    return this.order?.orderState === OrderState.Confirmed;
  }

  canUpdateToCustomerReceived(): boolean {
    return this.order?.orderState === OrderState.OnWay;
  }

  canComplete(): boolean {
    return this.order?.orderState === OrderState.CustomerReceived;
  }

  canCancel(): boolean {
    return this.order?.orderState === OrderState.Pending || this.order?.orderState === OrderState.Confirmed;
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

  isActionLoading(action: string): boolean {
    return this.actionLoading === action;
  }

  get OrderState() {
    return OrderState;
  }

  get PaymentMethod() {
    return PaymentMethod;
  }

  get PaymentState() {
    return PaymentState;
  }

}

