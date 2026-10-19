using System.Linq.Expressions;
using Application.Common;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Infrastructure.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Core.Tests.Infrastructure.Jobs;

public class AuditLogCleanupJobTests
{
    private readonly Mock<IAuditLogRepository> _repo = new();
    private readonly Mock<IDateTimeProvider> _clock = new();

    private AuditLogCleanupJob BuildJob(AuditLogRetentionSettings settings)
        => new(
            _repo.Object,
            _clock.Object,
            Options.Create(settings),
            NullLogger<AuditLogCleanupJob>.Instance);

    [Fact]
    public async Task ExecuteAsync_ShouldSkip_WhenDisabled()
    {
        var job = BuildJob(new AuditLogRetentionSettings { Enabled = false });

        await job.ExecuteAsync();

        _repo.Verify(r => r.GetAllAsync(
            It.IsAny<Expression<Func<AuditLog, bool>>>(),
            It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDoNothing_WhenNoExpiredLogs()
    {
        _clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1));

        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<AuditLog>());

        var job = BuildJob(new AuditLogRetentionSettings
        {
            Enabled = true,
            RetentionDays = 365,
            BatchSize = 5000
        });

        await job.ExecuteAsync();

        _repo.Verify(r => r.Delete(It.IsAny<AuditLog>()), Times.Never);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldDeleteExpiredLogs_WhenFound()
    {
        _clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1));

        var expiredLogs = new List<AuditLog>
        {
            new AuditLog("Test1", "Customer", true),
            new AuditLog("Test2", "Product", true)
        };

        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredLogs);

        var job = BuildJob(new AuditLogRetentionSettings
        {
            Enabled = true,
            RetentionDays = 90,
            BatchSize = 100
        });

        await job.ExecuteAsync();

        _repo.Verify(r => r.Delete(It.IsAny<AuditLog>()), Times.Exactly(2));
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldRespectBatchSize()
    {
        _clock.SetupGet(c => c.UtcNow).Returns(new DateTime(2026, 1, 1));

        var expiredLogs = Enumerable.Range(0, 10)
            .Select(i => new AuditLog($"Test{i}", "Customer", true))
            .ToList();

        _repo.Setup(r => r.GetAllAsync(
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredLogs);

        var job = BuildJob(new AuditLogRetentionSettings
        {
            Enabled = true,
            RetentionDays = 90,
            BatchSize = 3
        });

        await job.ExecuteAsync();

        _repo.Verify(r => r.Delete(It.IsAny<AuditLog>()), Times.Exactly(3));
    }
}
