using CSharpFunctionalExtensions;
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
    public record CreateProductCommand : IRequest<Result<int>>
    {
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        private class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, Result<int>>
        {
            private readonly DatabaseContext _context;

            public CreateProductCommandHandler(DatabaseContext context)
            {
                _context = context;
            }
            public async Task<Result<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
            {
                var category = await _context.Categories.AsTracking().FirstOrDefaultAsync(x => x.CategoryId == request.CategoryId);
                if (category is null)
                {
                    return Result.Failure<int>("null");
                }


                var newProductResult = category.AddProduct(
                                                        request.ProductName,
                                                        request.ProductDescription,
                                                        request.Price,
                                                        request.ImageUrl
                                                     );
                if (newProductResult.IsFailure)
                {
                    return Result.Failure<int>(newProductResult.Error);
                }

                var newCProduct = newProductResult.Value;
                var saveChanges = await _context.SaveChangesAsyncWithResult();
                if(!saveChanges.IsSuccess)
                {
                    return Result.Failure<int>("Server Error");
                }
                return Result.Success(newCProduct.ProductId);
            }
        }
    }
}
