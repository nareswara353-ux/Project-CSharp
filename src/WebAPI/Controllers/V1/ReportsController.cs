using Application.Reports;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.Reports)]
[Authorize(Roles = "Admin,Manager")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("sales")]
    [ProducesResponseType(typeof(SalesReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetSalesReport(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null)
    {
        var query = new SalesReportQuery
        {
            From = from ?? DateTime.UtcNow.AddDays(-30),
            To = to ?? DateTime.UtcNow
        };

        var result = await _mediator.Send(query);

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }

    [HttpGet("inventory")]
    [ProducesResponseType(typeof(InventoryReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInventoryReport([FromQuery] int lowStockThreshold = 10)
    {
        var query = new InventoryReportQuery { LowStockThreshold = lowStockThreshold };
        var result = await _mediator.Send(query);

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }
}
