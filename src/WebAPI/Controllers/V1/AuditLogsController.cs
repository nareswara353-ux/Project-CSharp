using Application.Common;
using Asp.Versioning;
using Domain.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Attributes;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auditlogs")]
[Authorize]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogRepository _auditLogRepository;

    public AuditLogsController(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    [HttpGet]
    [HasPermission(Permissions.Admin.ViewAuditLog)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationRequest
        {
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        var items = await _auditLogRepository.GetPagedAsync(
            pagination.PageNumber,
            pagination.PageSize,
            predicate: null,
            orderBy: q => q.OrderByDescending(a => a.OccurredOn),
            cancellationToken);

        var total = await _auditLogRepository.CountAsync(null, cancellationToken);

        var response = new PagedResult<AuditLogDto>(
            items.Select(MapToDto).ToList(),
            pagination.PageNumber,
            pagination.PageSize,
            total);

        return Ok(response);
    }

    [HttpGet("recent")]
    [HasPermission(Permissions.Admin.ViewAuditLog)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecent(
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetRecentAsync(take, cancellationToken);
        return Ok(logs.Select(MapToDto));
    }

    [HttpGet("user/{userId:guid}")]
    [HasPermission(Permissions.Admin.ViewAuditLog)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByUser(
        Guid userId,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByUserIdAsync(userId, take, cancellationToken);
        return Ok(logs.Select(MapToDto));
    }

    [HttpGet("entity/{entityType}")]
    [HasPermission(Permissions.Admin.ViewAuditLog)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEntity(
        string entityType,
        [FromQuery] string? entityId = null,
        [FromQuery] int take = 100,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.GetByEntityAsync(
            entityType,
            entityId,
            take,
            cancellationToken);

        return Ok(logs.Select(MapToDto));
    }

    private static AuditLogDto MapToDto(Domain.Entities.AuditLog log) => new(
        log.Id,
        log.UserId,
        log.Username,
        log.Action,
        log.EntityType,
        log.EntityId,
        log.Success,
        log.ErrorMessage,
        log.IpAddress,
        log.CorrelationId,
        log.DurationMs,
        log.OccurredOn);
}

public record AuditLogDto(
    Guid Id,
    Guid? UserId,
    string? Username,
    string Action,
    string EntityType,
    string? EntityId,
    bool Success,
    string? ErrorMessage,
    string? IpAddress,
    string? CorrelationId,
    long DurationMs,
    DateTime OccurredOn);
