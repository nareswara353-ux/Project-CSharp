using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebAPI.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(
        IServiceScopeFactory scopeFactory,
        ILogger<DatabaseHealthCheck> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
                return HealthCheckResult.Healthy("Database connection is available.");

            _logger.LogWarning("Database health check failed: cannot connect");
            return HealthCheckResult.Unhealthy("Cannot connect to database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check threw an exception");
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }
    }
}
