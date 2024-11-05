using CSharpFunctionalExtensions;
using Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Category: AuditableEntity
    {
        public Category()
        {
            this.products = new List<Product>();
        }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public List<Product> products = new List<Product>();

        public Result<Product> AddProduct(
                                        string productName,
                                        string productDescription,
                                        int price,
                                        string imageUrl
                                        )
        {
            var newProduct = Product.Instance(
                                          productName,
                                          productDescription,
                                          price,
                                          imageUrl
                                     );
            this.products.Add(newProduct);
            return Result.Success(newProduct);
        }
    }
}
