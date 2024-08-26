import { Routes, provideRouter } from '@angular/router';
import { CartComponent } from './components/cart/cart.component';
import { ProductsComponent } from './components/Products/products-List/products.component';
import { ProductDetailsComponent } from './components/Products/product-details/product-details.component';
import { AddProductComponent } from './components/Products/add-product/add-product.component';

export const routes: Routes = [
  { path: '', redirectTo: '/products', pathMatch: 'full' },
  {path:'products', component:ProductsComponent},
  {path:'cart', component:CartComponent},
  {path:'products/productDetails/:id', component:ProductDetailsComponent},
  {path:'addProduct', component:AddProductComponent},
];

export const appRouterProviders = [provideRouter(routes)];
