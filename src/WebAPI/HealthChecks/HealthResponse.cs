namespace WebAPI.HealthChecks;

public record HealthResponse(
    string Status,
    DateTime Timestamp,
    double DurationMs,
    IReadOnlyDictionary<string, HealthCheckEntry> Checks);

public record HealthCheckEntry(
    string Status,
    string? Description,
    double DurationMs,
    IReadOnlyDictionary<string, object>? Data);
