using CSharpFunctionalExtensions;
using Infrastructure;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Query.LookUps
{
    public sealed record CategoryNamesQueryLookup: IRequest<Result<List<CategoryLookupVm>>>
    {
        private class CategoryNamesQueryLookupHandler : IRequestHandler<CategoryNamesQueryLookup, Result<List<CategoryLookupVm>>>
        {
            private readonly DatabaseContext _context;

            public CategoryNamesQueryLookupHandler(DatabaseContext context)
            {
                _context = context;
            }
            public async Task<Result<List<CategoryLookupVm>>> Handle(CategoryNamesQueryLookup request, CancellationToken cancellationToken)
            {
                var category = await _context.Categories.Select(x => new CategoryLookupVm
                {
                    CategoryId = x.CategoryId,
                    CategoryName = x.CategoryName
                }).ToListAsync();
                if(category is null)
                {
                    return Result.Failure<List<CategoryLookupVm>>("No categories found");
                }
                return Result.Success(category);
            }
        }
    }
}
