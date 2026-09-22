using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Common.Behaviors;

public interface ICacheableQuery
{
    string CacheKey { get; }
    TimeSpan? CacheExpiration { get; }
}

public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ICacheService _cache;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(
        ICacheService cache,
        ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ICacheableQuery cacheable)
            return await next();

        var cached = await _cache.GetAsync<TResponse>(cacheable.CacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for {CacheKey}", cacheable.CacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss for {CacheKey}", cacheable.CacheKey);

        var response = await next();

        if (response is not null)
        {
            await _cache.SetAsync(
                cacheable.CacheKey,
                response,
                cacheable.CacheExpiration,
                cancellationToken);
        }

        return response;
    }
}
