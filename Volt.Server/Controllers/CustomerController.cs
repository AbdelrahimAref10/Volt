using Application.Features.Customer.Command.ActivateCustomerCommand;
using Application.Features.Customer.Command.CustomerLoginCommand;
using Application.Features.Customer.Command.RefreshTokenCommand;
using Application.Features.Customer.Command.RegisterCustomerCommand;
using Application.Features.City.DTOs;
using Application.Features.City.Query.GetCitiesLookupQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Volt.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CustomerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(RegisterCustomerResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Activate")]
        [AllowAnonymous]
        public async Task<IActionResult> Activate(ActivateCustomerCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(CustomerLoginCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpPost]
        [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("RefreshToken")]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshToken(RefreshTokenCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<CityLookupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("GetActiveCities")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveCities()
        {
            var query = new GetCitiesLookupQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }
    }
}

