import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CategoryClient, CategoryLookupVm, CreateProductCommand, ProductClient } from '../../core/services/clientAPI';
import { DxDataGridModule, DxLookupModule } from 'devextreme-angular';

@Component({
  selector: 'app-add-product',
  standalone: true,
  imports: [ReactiveFormsModule,DxDataGridModule,DxLookupModule ],
  providers:[ProductClient],
  templateUrl: './add-product.component.html',
  styleUrl: './add-product.component.css'
})
export class AddProductComponent {
  ProductForm!:FormGroup;
  productData:any;
  categoriesNames: CategoryLookupVm[] = [];
  constructor(private fb: FormBuilder, private productClient: ProductClient, private categoryClient: CategoryClient){

  }

  ngOnInit(){
    this.GetCategoryName();
    this.ProductForm = this.fb.group({
      ProdName:['', Validators.required],
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
        console.log("an error occurred", err);
      },
      complete: () => {
        console.log('Request complete');
      }
    })
  }

  onSubmit(data: any){
    this.productData = data;
    console.log("this data: ",data);
    var createProductCommand = new CreateProductCommand();
    createProductCommand.productName = data.ProdName;
    createProductCommand.productDescription = data.prodDecription;
    createProductCommand.price = data.price;
    createProductCommand.categoryId = data.categoryId;
    return this.productClient.createProduct(createProductCommand).subscribe({
      next:(response) => {
        console.log("Product Created Successfully");
      },
      error:(err) => {
        console.log("an error occurred", err);
      },
      complete: () => {
        console.log('Request complete');
      }
    });
  }
}
