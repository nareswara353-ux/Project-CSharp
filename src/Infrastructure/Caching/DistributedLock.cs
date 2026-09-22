namespace Infrastructure.Caching;

public interface IDistributedLock
{
    Task<bool> AcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default);

    Task ReleaseAsync(string resource, CancellationToken cancellationToken = default);

    Task<IAsyncDisposable?> AcquireOrWaitAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default);
}
