using System.Linq.Expressions;
using Application.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class InMemoryBackgroundJobService : IBackgroundJobService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<InMemoryBackgroundJobService> _logger;

    public InMemoryBackgroundJobService(
        IServiceScopeFactory scopeFactory,
        ILogger<InMemoryBackgroundJobService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull
    {
        _ = Task.Run(async () =>
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var instance = scope.ServiceProvider.GetRequiredService<T>();
                var method = ((MethodCallExpression)methodCall.Body).Method;
                var task = (Task?)method.Invoke(instance, Array.Empty<object>());
                if (task is not null)
                    await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Background job {Job} failed", typeof(T).Name);
            }
        });
    }

    public void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : notnull
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(delay);
                using var scope = _scopeFactory.CreateScope();
                var instance = scope.ServiceProvider.GetRequiredService<T>();
                var method = ((MethodCallExpression)methodCall.Body).Method;
                var task = (Task?)method.Invoke(instance, Array.Empty<object>());
                if (task is not null)
                    await task;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled job {Job} failed", typeof(T).Name);
            }
        });
    }

    public void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression) where T : notnull
    {
        _logger.LogInformation(
            "Recurring job {JobId} registered with schedule {Cron} (no-op in memory implementation)",
            jobId,
            cronExpression);
    }
}
