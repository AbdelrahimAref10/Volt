import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';

@Component({
  selector: 'app-update-product',
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: './update-product.component.html',
  styleUrl: './update-product.component.css'
})
export class UpdateProductComponent {
  productForm!: FormGroup
  constructor(private fb: FormBuilder){}
  ngOnInit(){
    this.productForm = this.fb.group({
      ProdName:['', Validators.required],
      imgUrl:[[]],
      prodDecription:[[]],
      categoryId:['', Validators.required],
      price:['', Validators.required],
    })
  }

  onSubmit(data: any){
          
  }
}
