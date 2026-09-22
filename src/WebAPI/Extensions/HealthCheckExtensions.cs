using Microsoft.Extensions.Diagnostics.HealthChecks;
using StackExchange.Redis;
using WebAPI.HealthChecks;

namespace WebAPI.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApplicationHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<StartupHealthCheck>();

        var builder = services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(
                "startup",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthCheckTags.Live, HealthCheckTags.Ready })
            .AddCheck<DatabaseHealthCheck>(
                "database",
                failureStatus: HealthStatus.Unhealthy,
                tags: new[] { HealthCheckTags.Ready, HealthCheckTags.Database })
            .AddCheck<DiskSpaceHealthCheck>(
                "disk",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { HealthCheckTags.Live, HealthCheckTags.Storage });

        var cachingProvider = configuration["Caching:Provider"];
        if (string.Equals(cachingProvider, "Redis", StringComparison.OrdinalIgnoreCase))
        {
            builder.AddCheck<RedisHealthCheck>(
                "redis",
                failureStatus: HealthStatus.Degraded,
                tags: new[] { HealthCheckTags.Ready, HealthCheckTags.Cache });
        }

        return services;
    }

    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        endpoints.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Live),
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains(HealthCheckTags.Ready),
            ResponseWriter = HealthCheckResponseWriter.WriteAsync
        });

        return endpoints;
    }
}
