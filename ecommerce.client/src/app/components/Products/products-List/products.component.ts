import { Component } from '@angular/core';
import { Observable } from 'rxjs';
import { AsyncPipe } from '@angular/common';
import { CartService } from '../../../core/services/cart.service';
import { RouterLink, RouterModule } from '@angular/router';
import { ProductClient, ProductsVm } from '../../../core/services/clientAPI';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [AsyncPipe, RouterLink, RouterModule],
  providers:[ProductClient, CartService],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css'
})
export class ProductsComponent {
  allProducts: ProductsVm[] = [];
  constructor(private productClient: ProductClient, private cartService: CartService ){}

  AddToCart(product: ProductsVm){
    this.cartService.add(product)
  }

  ngOnInit(){
    return this.productClient.getAllProducts().subscribe(data=>{
      this.allProducts = data;
    })
  }
}
