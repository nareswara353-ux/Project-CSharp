using System.Linq.Expressions;

namespace Application.Common;

public interface IBackgroundJobService
{
    void Enqueue<T>(Expression<Func<T, Task>> methodCall);

    void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);

    void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression);
}

public static class JobIds
{
    public const string OrderCleanup = "order-cleanup";
    public const string DailyReport = "daily-report";
    public const string StaleDraftCleanup = "stale-draft-cleanup";
}
