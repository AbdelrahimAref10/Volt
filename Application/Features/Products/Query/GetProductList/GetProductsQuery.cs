using Application.Features.Products.Query.GetProductList;
using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Products.Query.GetProductList
{
    public sealed record GetProductsQuery : IRequest<Result<List<ProductsVm>>>
    {
        private class GetProductsQueryHandler : IRequestHandler<GetProductsQuery, Result<List<ProductsVm>>>
        {
            private readonly DatabaseContext _dbContext;

            public GetProductsQueryHandler(DatabaseContext context)
            {
                _dbContext = context;
            }

            public async Task<Result<List<ProductsVm>>> Handle(GetProductsQuery request, CancellationToken cancellationToken)
            {
                var product = await _dbContext.Products.Select(x => new ProductsVm
                {
                    ProductId = x.ProductId,
                    ProductName = x.ProductName,
                    ProductDescription = x.ProductDescription,
                    Price = x.Price,
                    CategoryName = x.Category.CategoryName,
                }).ToListAsync();

                if(product is null)
                {
                    return Result.Failure<List<ProductsVm>>("No Products Found");
                }

                return Result.Success(product);
            }
        }

    }
}
