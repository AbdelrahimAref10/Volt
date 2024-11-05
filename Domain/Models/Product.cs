using Domain.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Product: AuditableEntity
    {
        public int ProductId {  get; private set; }
        public string ProductName { get; private set; } = string.Empty;
        public string ProductDescription { get; private set;} = string.Empty;
        public int Price { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public int CategoryId {  get; private set; }

        public Category Category = new Category();

        public static Product Instance(
                                        string productName,
                                        string productDescription,
                                        int price,
                                        string imageUrl
                                        )
        {
            var product = new Product
            {
                ProductName = productName,
                ProductDescription = productDescription,
                Price = price,
                ImageUrl = imageUrl
            };
            return product;
        }  

        public void UpdateProduct(string productName, string productDescription,int price, string imageUrl, int categoryId)
        {
            this.ProductName = productName;
            this.ProductDescription = productDescription;
            this.Price = price;
            this.ImageUrl = imageUrl;
            this.CategoryId = categoryId;
        }
    }
}
