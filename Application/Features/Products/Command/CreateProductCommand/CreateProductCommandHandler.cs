using Application.Features.Products.Query.GetProductList;
using CSharpFunctionalExtensions;
using Domain.Models;
using Domain.Shared;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Products.Command.CreateProductCommand
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
    {
        private readonly DatabaseContext _context;

        public CreateProductCommandHandler(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var category = await _context.Categories.AsTracking().FirstOrDefaultAsync(x => x.CategoryId == request.CategoryId);
                if (category is null)
                {
                    return Result.Failure<int>("null");
                }

                var product = new Product
                {
                    ProductName = request.ProductName,
                    ProductDescription = request.ProductDescription,
                    Price = request.Price,
                };

                category.AddProduct(product);
                await _context.SaveChangesAsync();
                return Result.Success(product.ProductId);
            }
            catch (Exception ex)
            {
                return Result.Failure<int>(ex.Message);
            }
        }
    }
}
