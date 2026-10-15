using Domain.Entities;

namespace Application.Auditing;

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
    DateTime OccurredOn)
{
    public static AuditLogDto FromEntity(AuditLog log) => new(
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
