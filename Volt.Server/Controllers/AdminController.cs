using Application.Features.Admin.Command.AdminLoginCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Response;

namespace Volt.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AdminLoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        [Route("Login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(AdminLoginCommand command)
        {
            var result = await _mediator.Send(command);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }
    }
}

