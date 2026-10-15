using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using MediatR;

namespace Application.Auditing;

public record GetAuditLogsQuery : IRequest<Result<PagedResult<AuditLogDto>>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public Guid? UserId { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }
}

public class GetAuditLogsQueryHandler
    : IRequestHandler<GetAuditLogsQuery, Result<PagedResult<AuditLogDto>>>
{
    private readonly IAuditLogRepository _auditLogRepository;

    public GetAuditLogsQueryHandler(IAuditLogRepository auditLogRepository)
    {
        _auditLogRepository = auditLogRepository;
    }

    public async Task<Result<PagedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
        try
        {
            var pagination = new PaginationRequest
            {
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            };

            System.Linq.Expressions.Expression<Func<AuditLog, bool>> predicate = a =>
                (!request.UserId.HasValue || a.UserId == request.UserId.Value) &&
                (string.IsNullOrWhiteSpace(request.EntityType) || a.EntityType == request.EntityType) &&
                (string.IsNullOrWhiteSpace(request.EntityId) || a.EntityId == request.EntityId) &&
                (!request.From.HasValue || a.OccurredOn >= request.From.Value) &&
                (!request.To.HasValue || a.OccurredOn <= request.To.Value);

            var items = await _auditLogRepository.GetPagedAsync(
                pagination.PageNumber,
                pagination.PageSize,
                predicate,
                q => q.OrderByDescending(a => a.OccurredOn),
                cancellationToken);

            var total = await _auditLogRepository.CountAsync(predicate, cancellationToken);

            var dtos = items.Select(AuditLogDto.FromEntity).ToList();

            var result = new PagedResult<AuditLogDto>(
                dtos,
                pagination.PageNumber,
                pagination.PageSize,
                total);

            return Result<PagedResult<AuditLogDto>>.Success(result);
        }
        catch (Exception ex)
        {
            return Result<PagedResult<AuditLogDto>>.Failure(
                $"Failed to retrieve audit logs: {ex.Message}",
                "GET_AUDIT_LOGS_FAILED");
        }
    }
}
