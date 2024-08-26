import { Component, ViewChild } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryClient, CategoryLookupVm, CreateProductCommand, ProductClient } from '../../../core/services/clientAPI';
import { CommonModule, NgFor } from '@angular/common';
import { ToastContainerDirective, ToastrComponentlessModule, ToastrModule, ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [
    ReactiveFormsModule, 
    NgFor,
    ToastrModule,
    ToastrComponentlessModule,
    CommonModule,
    ToastContainerDirective
  ],
  providers:[ProductClient],
  templateUrl: './add-product.component.html',
  styleUrl: './add-product.component.css'
})
export class AddProductComponent {
  @ViewChild(ToastContainerDirective, { static: true })
  toastContainer: ToastContainerDirective | undefined;
  ProductForm!:FormGroup;
  productData:any;
  categoriesNames: CategoryLookupVm[] = [];
  constructor(private toastr: ToastrService,private fb: FormBuilder, private productClient: ProductClient, private categoryClient: CategoryClient){

  }

  ngOnInit(){
    this.toastr.overlayContainer = this.toastContainer;
    this.GetCategoryName();
    this.ProductForm = this.fb.group({
      ProdName:['', Validators.required],
      imgUrl:[[]],
      prodDecription:[[]],
      categoryId:['', Validators.required],
      price:['', Validators.required],
    });
  }

  GetCategoryName(){
    return this.categoryClient.getCategoryNames().subscribe({
      next:(response) => {
        this.categoriesNames = response;
      },
      error:(err) => {
        this.toastr.error('Categoty Creation Failed');
      },
    })
  }

  onSubmit(){
    const data = this.ProductForm.value;
    data.categoryId = parseInt(data.categoryId, 10);
    console.log("this data: ",data);
    var createProductCommand = new CreateProductCommand();
      createProductCommand.productName = data.ProdName;
      createProductCommand.productDescription = data.prodDecription;
      createProductCommand.price = data.price;
      createProductCommand.imageUrl = data.imgUrl;
      createProductCommand.categoryId = data.categoryId;
    this.productClient.createProduct(createProductCommand).subscribe({
      next:(response) => {
        this.toastr.success('Product Created Successfully');
      },
      error:(err) => {
        this.toastr.error('Faild Creation');
      },
      complete: () => {
        console.log('Request complete');
      }
    });
  }
}
