namespace Infrastructure.Caching;

public class CachingSettings
{
    public const string SectionName = "Caching";

    public string Provider { get; set; } = "InMemory";
    public string? RedisConnectionString { get; set; }
    public int DefaultExpirationMinutes { get; set; } = 5;
}

public static class CacheProviders
{
    public const string None = "None";
    public const string InMemory = "InMemory";
    public const string Redis = "Redis";
}
