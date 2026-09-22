using Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Infrastructure.Caching;

public static class CacheServiceExtensions
{
    public static IServiceCollection AddCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(CachingSettings.SectionName)
            .Get<CachingSettings>() ?? new CachingSettings();

        switch (settings.Provider)
        {
            case CacheProviders.Redis:
                if (string.IsNullOrWhiteSpace(settings.RedisConnectionString))
                    throw new InvalidOperationException(
                        "Redis provider selected but RedisConnectionString is missing.");

                services.AddSingleton<IConnectionMultiplexer>(_ =>
                    ConnectionMultiplexer.Connect(settings.RedisConnectionString));
                services.AddSingleton<ICacheService, RedisCacheService>();
                break;

            case CacheProviders.InMemory:
                services.AddMemoryCache();
                services.AddSingleton<ICacheService, InMemoryCacheService>();
                break;

            case CacheProviders.None:
            default:
                services.AddSingleton<ICacheService, NoOpCacheService>();
                break;
        }

        return services;
    }
}
