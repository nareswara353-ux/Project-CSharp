using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace WebAPI.HealthChecks;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new HealthResponse(
            Status: report.Status.ToString(),
            Timestamp: DateTime.UtcNow,
            DurationMs: report.TotalDuration.TotalMilliseconds,
            Checks: report.Entries.ToDictionary(
                kvp => kvp.Key,
                kvp => new HealthCheckEntry(
                    Status: kvp.Value.Status.ToString(),
                    Description: kvp.Value.Description,
                    DurationMs: kvp.Value.Duration.TotalMilliseconds,
                    Data: kvp.Value.Data.ToDictionary(d => d.Key, d => d.Value))));

        return context.Response.WriteAsync(JsonSerializer.Serialize(response, Options));
    }
}
