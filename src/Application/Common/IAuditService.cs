namespace Application.Common;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public record AuditEntry(
    string Action,
    string EntityType,
    bool Success,
    Guid? UserId = null,
    string? Username = null,
    string? EntityId = null,
    string? Payload = null,
    string? ErrorMessage = null,
    string? IpAddress = null,
    string? UserAgent = null,
    string? CorrelationId = null,
    long DurationMs = 0);
