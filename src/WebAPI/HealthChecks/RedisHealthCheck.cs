using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;

namespace WebAPI.HealthChecks;

public class RedisHealthCheck : IHealthCheck
{
    private readonly IConnectionMultiplexer? _redis;

    public RedisHealthCheck(IConnectionMultiplexer? redis = null)
    {
        _redis = redis;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_redis is null)
            return HealthCheckResult.Degraded("Redis is not configured.");

        try
        {
            var db = _redis.GetDatabase();
            var latency = await db.PingAsync();

            var data = new Dictionary<string, object>
            {
                ["LatencyMs"] = latency.TotalMilliseconds,
                ["Endpoints"] = string.Join(",", _redis.GetEndPoints().Select(e => e.ToString()))
            };

            return HealthCheckResult.Healthy("Redis is responsive.", data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis ping failed: {ex.Message}");
        }
    }
}
