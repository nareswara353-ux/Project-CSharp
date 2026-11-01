using System.Text.Json;
using Application.Common;

namespace WebAPI.Middleware;

public sealed class MaintenanceModeMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<MaintenanceModeMiddleware> _logger;

    public MaintenanceModeMiddleware(
        RequestDelegate next,
        ILogger<MaintenanceModeMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IFeatureFlagService featureFlags)
    {
        if (IsBypassedPath(context.Request.Path))
        {
            await _next(context);
            return;
        }

        bool maintenance;
        try
        {
            maintenance = await featureFlags.IsEnabledAsync(
                FeatureFlags.MaintenanceMode,
                defaultValue: false,
                context.RequestAborted);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check maintenance flag, allowing request");
            await _next(context);
            return;
        }

        if (!maintenance)
        {
            await _next(context);
            return;
        }

        _logger.LogWarning(
            "Request blocked by maintenance mode: {Method} {Path}",
            context.Request.Method,
            context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        context.Response.ContentType = "application/json";
        context.Response.Headers["Retry-After"] = "300";

        var body = new
        {
            success = false,
            message = "Service is temporarily unavailable for maintenance. Please try again later.",
            errorCode = "MAINTENANCE_MODE",
            retryAfterSeconds = 300
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(body, JsonOptions));
    }

    private static bool IsBypassedPath(PathString path)
    {
        var value = path.Value ?? string.Empty;

        if (value.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.Contains("/auth/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.Contains("/featureflags", StringComparison.OrdinalIgnoreCase))
            return true;

        if (value.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
            return true;

        return false;
    }
}
