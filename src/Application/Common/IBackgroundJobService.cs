using System.Linq.Expressions;

namespace Application.Common;

public interface IBackgroundJobService
{
    void Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull;

    void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : notnull;

    void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression) where T : notnull;
}

public static class JobIds
{
    public const string OrderCleanup = "order-cleanup";
    public const string DailyReport = "daily-report";
    public const string StaleDraftCleanup = "stale-draft-cleanup";
}
