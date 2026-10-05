using Domain.Entities;
using Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Repositories;

public class EfAuditLogRepository : EfRepository<AuditLog>, IAuditLogRepository
{
    public EfAuditLogRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<AuditLog>> GetByUserIdAsync(
        Guid userId,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.OccurredOn)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        string? entityId = null,
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(a => a.EntityType == entityType);

        if (!string.IsNullOrWhiteSpace(entityId))
            query = query.Where(a => a.EntityId == entityId);

        return await query
            .OrderByDescending(a => a.OccurredOn)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetRecentAsync(
        int take = 100,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .OrderByDescending(a => a.OccurredOn)
            .Take(take)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .AsNoTracking()
            .Where(a => a.OccurredOn >= from && a.OccurredOn <= to)
            .OrderByDescending(a => a.OccurredOn)
            .ToListAsync(cancellationToken);
    }
}
