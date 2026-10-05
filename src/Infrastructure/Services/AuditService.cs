using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<AuditService> _logger;

    public AuditService(
        IAuditLogRepository repository,
        ILogger<AuditService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        try
        {
            var log = new AuditLog(
                action: entry.Action,
                entityType: entry.EntityType,
                success: entry.Success,
                userId: entry.UserId,
                username: entry.Username,
                entityId: entry.EntityId,
                payload: entry.Payload,
                errorMessage: entry.ErrorMessage,
                ipAddress: entry.IpAddress,
                userAgent: entry.UserAgent,
                correlationId: entry.CorrelationId,
                durationMs: entry.DurationMs);

            await _repository.AddAsync(log, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to write audit log for action {Action} on {EntityType}",
                entry.Action,
                entry.EntityType);
        }
    }
}
