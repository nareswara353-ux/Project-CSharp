namespace Infrastructure.Caching;

public class NoOpDistributedLock : IDistributedLock
{
    public Task<bool> AcquireAsync(
        string resource,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
        => Task.FromResult(true);

    public Task ReleaseAsync(string resource, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    public Task<IAsyncDisposable?> AcquireOrWaitAsync(
        string resource,
        TimeSpan expiry,
        TimeSpan waitTimeout,
        CancellationToken cancellationToken = default)
        => Task.FromResult<IAsyncDisposable?>(new NoOpDisposable());

    private sealed class NoOpDisposable : IAsyncDisposable
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
