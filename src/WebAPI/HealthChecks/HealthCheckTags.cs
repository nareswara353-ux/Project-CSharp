namespace WebAPI.HealthChecks;

public static class HealthCheckTags
{
    public const string Ready = "ready";
    public const string Live = "live";
    public const string Database = "db";
    public const string Cache = "cache";
    public const string External = "external";
    public const string Storage = "storage";
}
