using Application.Features.AdminDashboard.DTOs;
using Application.Features.AdminDashboard.Query.GetAdminDashboardAnalyticsQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Response;

namespace Volt.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminDashboardController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Get comprehensive admin dashboard analytics
        /// </summary>
        /// <param name="recentOrdersCount">Number of recent orders to return (default: 10)</param>
        /// <param name="topCategoriesCount">Number of top categories to return (default: 5)</param>
        /// <param name="revenuePeriodsCount">Number of revenue periods to return (default: 6 months)</param>
        /// <returns>Comprehensive dashboard analytics</returns>
        [HttpGet("Analytics")]
        [ProducesResponseType(typeof(AdminDashboardAnalyticsDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetDashboardAnalytics(
            [FromQuery] int recentOrdersCount = 10,
            [FromQuery] int topCategoriesCount = 5,
            [FromQuery] int revenuePeriodsCount = 6)
        {
            var query = new GetAdminDashboardAnalyticsQuery
            {
                RecentOrdersCount = recentOrdersCount,
                TopCategoriesCount = topCategoriesCount,
                RevenuePeriodsCount = revenuePeriodsCount
            };

            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }

            return Ok(result.Value);
        }
    }
}
