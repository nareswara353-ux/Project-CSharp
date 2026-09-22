using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Infrastructure.Caching;

public class RedisDistributedLock : IDistributedLock
{
    private const string ReleaseScript = @"
        if redis.call('get', KEYS[1]) == ARGV[1] then
            return redis.call('del', KEYS[1])
        else
            return 0
        end";

    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisDistributedLock> _logger;

    public RedisDistributedLock(
        IConnectionMultiplexer redis,
        ILogger<RedisDistributedLock> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> AcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var token = Guid.NewGuid().ToString("N");

        var acquired = await db.StringSetAsync(
            resource,
            token,
            expiry,
            When.NotExists);

        if (acquired)
        {
            _logger.LogDebug("Acquired lock {Resource}", resource);
            return true;
        }

        return false;
    }

    public async Task ReleaseAsync(string resource, CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        try
        {
            await db.ScriptEvaluateAsync(ReleaseScript, new RedisKey[] { resource });
            _logger.LogDebug("Released lock {Resource}", resource);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to release lock {Resource}", resource);
        }
    }

    public async Task<IAsyncDisposable?> AcquireOrWaitAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
    {
        var deadline = DateTime.UtcNow.Add(waitTimeout);
        var delay = TimeSpan.FromMilliseconds(100);

        while (DateTime.UtcNow < deadline)
        {
            if (await AcquireAsync(resource, expiry, cancellationToken))
                return new Releaser(this, resource);

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromMilliseconds(Math.Min(delay.TotalMilliseconds * 2, 1000));
        }

        return null;
    }

    private sealed class Releaser : IAsyncDisposable
    {
        private readonly RedisDistributedLock _lock;
        private readonly string _resource;

        public Releaser(RedisDistributedLock @lock, string resource)
        {
            _lock = @lock;
            _resource = resource;
        }

        public ValueTask DisposeAsync()
            => new(_lock.ReleaseAsync(_resource));
    }
}
