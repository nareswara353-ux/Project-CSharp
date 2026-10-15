using Application.Common;
using Domain.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Jobs;

public class AuditLogCleanupJob
{
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly AuditLogRetentionSettings _settings;
    private readonly ILogger<AuditLogCleanupJob> _logger;

    public AuditLogCleanupJob(
        IAuditLogRepository auditLogRepository,
        IDateTimeProvider dateTimeProvider,
        IOptions<AuditLogRetentionSettings> settings,
        ILogger<AuditLogCleanupJob> logger)
    {
        _auditLogRepository = auditLogRepository;
        _dateTimeProvider = dateTimeProvider;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task ExecuteAsync()
    {
        if (!_settings.Enabled)
        {
            _logger.LogDebug("AuditLogCleanupJob disabled, skipping");
            return;
        }

        _logger.LogInformation(
            "AuditLogCleanupJob started (retention: {Days} days)",
            _settings.RetentionDays);

        try
        {
            var cutoff = _dateTimeProvider.UtcNow.AddDays(-_settings.RetentionDays);

            var expiredLogs = await _auditLogRepository.GetAllAsync(
                predicate: a => a.OccurredOn < cutoff,
                orderBy: null,
                cancellationToken: CancellationToken.None);

            if (expiredLogs.Count == 0)
            {
                _logger.LogInformation("AuditLogCleanupJob: no expired logs found");
                return;
            }

            var toDelete = expiredLogs.Take(_settings.BatchSize).ToList();
            var totalDeleted = 0;

            foreach (var log in toDelete)
            {
                _auditLogRepository.Delete(log);
                totalDeleted++;
            }

            await _auditLogRepository.SaveChangesAsync();

            _logger.LogInformation(
                "AuditLogCleanupJob completed: deleted {Deleted} of {Total} expired logs",
                totalDeleted,
                expiredLogs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "AuditLogCleanupJob failed");
            throw;
        }
    }
}
