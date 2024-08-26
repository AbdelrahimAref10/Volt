import { Component } from '@angular/core';
import { ActivatedRoute, RouterLink, RouterModule } from '@angular/router';
import { ProductByIdVm, ProductClient } from '../../../core/services/clientAPI';
import { Observable, Subscription } from 'rxjs';
import { AsyncPipe } from '@angular/common';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [AsyncPipe,RouterModule, RouterLink],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css'
})
export class ProductDetailsComponent {
  subscribe!: Subscription;
  productId: number = 0;
  productDetails: ProductByIdVm = new ProductByIdVm();
  constructor(private router: ActivatedRoute, private productClient: ProductClient){
    this.productId = this.router.snapshot.params['id'];
  }
  ngOnInit(){
    this.subscribe = this.productClient.getProductDetails(this.productId).subscribe(data => {
      this.productDetails = data
    });
  }
  ngOnDestroy(): void {
    this.subscribe.unsubscribe();
  }
}
