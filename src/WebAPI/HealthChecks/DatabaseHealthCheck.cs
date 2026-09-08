using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Data;

namespace WebAPI.HealthChecks;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(IServiceProvider serviceProvider, ILogger<DatabaseHealthCheck> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            // Try to execute a simple query to verify connection
            var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (canConnect)
            {
                _logger.LogInformation("Database health check succeeded");
                return HealthCheckResult.Healthy("Database connection is available.");
            }

            _logger.LogWarning("Database health check failed: Cannot connect to database");
            return HealthCheckResult.Unhealthy("Cannot connect to database.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed with exception");
            return HealthCheckResult.Unhealthy($"Database connection failed: {ex.Message}");
        }
    }
}
