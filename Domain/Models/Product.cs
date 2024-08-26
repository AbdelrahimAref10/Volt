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
        public int ProductId {  get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set;} = string.Empty;
        public int Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId {  get; set; }

        public Category Category = new Category();

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
