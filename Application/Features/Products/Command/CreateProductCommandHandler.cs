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

namespace Application.Features.Products.Command
{
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, ResponseModel<bool>>
    {
        private readonly DatabaseContext _context;

        public CreateProductCommandHandler(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<ResponseModel<bool>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var product = new Product
                {
                    ProductName = request.ProductName,
                    ProductDescription = request.ProductDescription,
                    Price = request.Price,
                    CategoryId = request.CategoryId
                };
                await _context.Products.AddAsync(product, cancellationToken);
                await _context.SaveChangesAsync();
                return new ResponseModel<bool>
                {
                    IsSuccess = true,
                    Data = true, 
                    ErrorMessege = "Product created successfully."

                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<bool>
                {
                    IsSuccess = false,
                    Data = false, 
                    ErrorMessege = $"An error occurred while creating the product: {ex.Message}"
                };
            }
        }
    }
}
