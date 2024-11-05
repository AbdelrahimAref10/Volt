using Application.Features.Products.Command.CreateProductCommand;
using Application.Features.Products.Command.UpdateProductCommand;
using Application.Features.Products.Query.GetProductById;
using Application.Features.Products.Query.GetProductList;
using Azure;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Namotion.Reflection;
using Presentation.Response;


namespace ECommerce.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<ProductsVm>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("AllProducts")]
        public async Task<IActionResult> GetAllProducts()
        {
            var result = await _mediator.Send(new GetProductsQuery());
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }


        [HttpPost]
        [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]

        [Route("CreateProduct")]
        public async Task<IActionResult> CreateProduct(CreateProductCommand product)
        {
            var result = await _mediator.Send(product);

            if (!result.IsSuccess)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesDefaultResponseType]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]

        [Route("UpdateProduct")]
        public async Task<IActionResult> UpdateProduct(UpdateProductCommand product)
        {
            var result = await _mediator.Send(product);

            if (!result.IsSuccess)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.IsSuccess);
        }


        [HttpGet]
        [ProducesResponseType(typeof(ProductByIdVm), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("ProductDetails")]
        public async Task<ActionResult> GetProductDetails(int productId)
        {
            var result = await _mediator.Send(new GetProductByIDQuery()
            {
                ProductId = productId,
            });

            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

    }
}
