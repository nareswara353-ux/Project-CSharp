using Application.Auditing;
using Application.Common;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Attributes;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditlogs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuditLogsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [HasPermission(Permissions.Admin.ViewAuditLog)]
    [ProducesResponseType(typeof(PagedResult<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] Guid? userId = null,
        [FromQuery] string? entityType = null,
        [FromQuery] string? entityId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditLogsQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            UserId = userId,
            EntityType = entityType,
            EntityId = entityId,
            From = from,
            To = to
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }
}

public static class ResultExtensions
{
    public static IActionResult Match<T>(
        this Result<T> result,
        Func<IActionResult> onSuccess,
        Func<IActionResult> onFailure)
        => result.IsSuccess ? onSuccess() : onFailure();
}
