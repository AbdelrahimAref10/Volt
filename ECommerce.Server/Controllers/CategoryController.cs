using Application.Features.Products.Query.GetProductList;
using Application.Query.LookUps;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Presentation.Response;

namespace ECommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoryLookupVm>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AllProducts")]
        public async Task<IActionResult> GetCategoryNames()
        {
            var result = await _mediator.Send(new CategoryNamesQueryLookup());
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

    }
}
