using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Features.Products.Command.UpdateProductCommand
{
    public sealed record UpdateProductCommand: IRequest<Result>
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductDescription { get; set; } = string.Empty;
        public int Price {  get; set;}
        public string ImageUrl { get; set; } = string.Empty;
        public int CategoryId {  get; set;}

        private class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Result>
        {
            private readonly DatabaseContext _context;

            public UpdateProductCommandHandler(DatabaseContext context)
            {
                _context = context;
            }
            public async Task<Result> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
            {
                var product = await _context.Products.AsTracking().FirstOrDefaultAsync(x => x.ProductId == request.ProductId);
                if (product is null)
                {
                    return Result.Failure("Failed To Update Product");
                }
                product.UpdateProduct(request.ProductName, request.ProductDescription, request.Price, request.ImageUrl, request.CategoryId);
                var saveResult = await _context.SaveChangesAsync();
                return Result.Success(product);
            }
        }



    }
}
