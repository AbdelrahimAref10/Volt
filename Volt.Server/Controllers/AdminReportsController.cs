using Application.Features.Order.DTOs;
using Application.Features.Order.Query.Reports.CancellationFeesReportQuery;
using Application.Features.Order.Query.Reports.CancellationReportQuery;
using Application.Features.Order.Query.Reports.CustomerOrderHistoryReportQuery;
using Application.Features.Order.Query.Reports.OrdersByDateRangeReportQuery;
using Application.Features.Order.Query.Reports.OrdersByStateReportQuery;
using Application.Features.Order.Query.Reports.RevenueByPeriodReportQuery;
using Application.Features.Order.Query.Reports.RevenueReportQuery;
using Application.Features.Order.Query.Reports.TreasuryBalanceReportQuery;
using Application.Features.Order.Query.Reports.VehicleUtilizationReportQuery;
using Application.Features.Order.Query.Reports.WalletReportQuery;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Presentation.Response;
using System.Collections.Generic;

namespace Volt.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AdminReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("OrdersByState")]
        [ProducesResponseType(typeof(List<OrdersByStateReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrdersByStateReport()
        {
            var query = new OrdersByStateReportQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("OrdersByDateRange")]
        [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetOrdersByDateRangeReport([FromQuery] OrdersByDateRangeReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("Revenue")]
        [ProducesResponseType(typeof(RevenueReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRevenueReport([FromQuery] RevenueReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("Cancellations")]
        [ProducesResponseType(typeof(CancellationReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCancellationReport()
        {
            var query = new CancellationReportQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("VehicleUtilization")]
        [ProducesResponseType(typeof(List<VehicleUtilizationReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetVehicleUtilizationReport()
        {
            var query = new VehicleUtilizationReportQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("CustomerOrderHistory")]
        [ProducesResponseType(typeof(List<OrderDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCustomerOrderHistoryReport([FromQuery] int customerId)
        {
            var query = new CustomerOrderHistoryReportQuery { CustomerId = customerId };
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("TreasuryBalance")]
        [ProducesResponseType(typeof(TreasuryReportDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetTreasuryBalanceReport()
        {
            var query = new TreasuryBalanceReportQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("RevenueByPeriod")]
        [ProducesResponseType(typeof(List<RevenueReportDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetRevenueByPeriodReport([FromQuery] RevenueByPeriodReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("CancellationFees")]
        [ProducesResponseType(typeof(List<CustomerWalletDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetCancellationFeesReport()
        {
            var query = new CancellationFeesReportQuery();
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }

        [HttpGet("Wallet")]
        [ProducesResponseType(typeof(List<WalletReportEntryDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetail), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetWalletReport([FromQuery] WalletReportQuery query)
        {
            var result = await _mediator.Send(query);
            if (result.IsFailure)
            {
                return BadRequest(ProblemDetail.CreateProblemDetail(result.Error));
            }
            return Ok(result.Value);
        }
    }
}
