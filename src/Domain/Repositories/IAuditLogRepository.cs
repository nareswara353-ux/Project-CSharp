using Domain.Entities;

namespace Domain.Repositories;

public interface IAuditLogRepository : IRepository<AuditLog>
{
    Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
        Guid userId,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        string? entityId = null,
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int take = 100,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);
}
