using Domain.Common;

namespace Domain.Entities;

public class AuditLog : Entity
{
    public Guid? UserId { get; private set; }
    public string? Username { get; private set; }
    public string Action { get; private set; } = null!;
    public string EntityType { get; private set; } = null!;
    public string? EntityId { get; private set; }
    public string? Payload { get; private set; }
    public bool Success { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? CorrelationId { get; private set; }
    public long DurationMs { get; private set; }
    public DateTime OccurredOn { get; private set; }

    private AuditLog()
    {
        Action = null!;
        EntityType = null!;
        OccurredOn = DateTime.UtcNow;
    }

    public AuditLog(
        string action,
        string entityType,
        bool success,
        Guid? userId = null,
        string? username = null,
        string? entityId = null,
        string? payload = null,
        string? errorMessage = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? correlationId = null,
        long durationMs = 0)
        : base()
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be empty", nameof(action));

        if (string.IsNullOrWhiteSpace(entityType))
            throw new ArgumentException("Entity type cannot be empty", nameof(entityType));

        Action = action.Trim();
        EntityType = entityType.Trim();
        Success = success;
        UserId = userId;
        Username = username;
        EntityId = entityId;
        Payload = payload;
        ErrorMessage = errorMessage;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        CorrelationId = correlationId;
        DurationMs = durationMs;
        OccurredOn = DateTime.UtcNow;
    }
}
