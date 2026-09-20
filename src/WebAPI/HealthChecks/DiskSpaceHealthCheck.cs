using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebAPI.HealthChecks;

public class DiskSpaceHealthCheck : IHealthCheck
{
    private const long HealthyBytes = 1L * 1024 * 1024 * 1024;
    private const long DegradedBytes = 500L * 1024 * 1024;
    private const long UnhealthyBytes = 100L * 1024 * 1024;

    private readonly ILogger<DiskSpaceHealthCheck> _logger;

    public DiskSpaceHealthCheck(ILogger<DiskSpaceHealthCheck> logger)
    {
        _logger = logger;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var drive = DriveInfo.GetDrives()
                .FirstOrDefault(d => d.IsReady && d.Name == Path.GetPathRoot(AppContext.BaseDirectory));

            if (drive is null)
                return Task.FromResult(HealthCheckResult.Degraded("Drive info unavailable."));

            var freeBytes = drive.AvailableFreeSpace;
            var data = new Dictionary<string, object>
            {
                ["FreeBytes"] = freeBytes,
                ["FreeMegabytes"] = freeBytes / 1024 / 1024,
                ["TotalBytes"] = drive.TotalSize
            };

            if (freeBytes >= HealthyBytes)
                return Task.FromResult(HealthCheckResult.Healthy("Disk space sufficient.", data));

            if (freeBytes >= DegradedBytes)
                return Task.FromResult(HealthCheckResult.Degraded("Disk space getting low.", data: data));

            if (freeBytes >= UnhealthyBytes)
                return Task.FromResult(HealthCheckResult.Degraded("Disk space critically low.", data: data));

            _logger.LogWarning("Disk space unhealthy: {FreeMb} MB free", freeBytes / 1024 / 1024);
            return Task.FromResult(HealthCheckResult.Unhealthy("Insufficient disk space.", data: data));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Disk check failed: {ex.Message}"));
        }
    }
}
