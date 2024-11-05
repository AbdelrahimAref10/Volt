import { Routes, provideRouter } from '@angular/router';
import { CartComponent } from './components/cart/cart.component';
import { ProductsComponent } from './components/Products/products-List/products.component';
import { ProductDetailsComponent } from './components/Products/product-details/product-details.component';
import { AddProductComponent } from './components/Products/add-product/add-product.component';
import { MainLayoutComponent } from './layout/main-layout/main-layout.component';

export const routes: Routes = [
  { path: '', redirectTo: '/main/products', pathMatch: 'full' },
  {
    path:'main',
    component: MainLayoutComponent,
    children:[
      {
        path:'products',
        component: ProductsComponent
      },
      {
        path:'cart', 
        component:CartComponent
      },
      {
        path:'products/productDetails/:id', 
        component:ProductDetailsComponent
      },
      {
        path:'addProduct', 
        component:AddProductComponent
      },
    ]
  },
];

export const appRouterProviders = [provideRouter(routes)];
