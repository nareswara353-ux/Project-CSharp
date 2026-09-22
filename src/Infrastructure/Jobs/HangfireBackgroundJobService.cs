using System.Linq.Expressions;
using Application.Common;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _jobClient;
    private readonly IRecurringJobManager _recurringJobManager;
    private readonly ILogger<HangfireBackgroundJobService> _logger;

    public HangfireBackgroundJobService(
        IBackgroundJobClient jobClient,
        IRecurringJobManager recurringJobManager,
        ILogger<HangfireBackgroundJobService> logger)
    {
        _jobClient = jobClient;
        _recurringJobManager = recurringJobManager;
        _logger = logger;
    }

    public void Enqueue<T>(Expression<Func<T, Task>> methodCall) where T : notnull
    {
        var jobId = _jobClient.Enqueue(methodCall);
        _logger.LogInformation("Enqueued job {JobId} of type {JobType}", jobId, typeof(T).Name);
    }

    public void Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) where T : notnull
    {
        var jobId = _jobClient.Schedule(methodCall, delay);
        _logger.LogInformation(
            "Scheduled job {JobId} of type {JobType} with delay {Delay}",
            jobId,
            typeof(T).Name,
            delay);
    }

    public void AddOrUpdateRecurring<T>(
        string jobId,
        Expression<Func<T, Task>> methodCall,
        string cronExpression) where T : notnull
    {
        _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression);
        _logger.LogInformation(
            "Registered recurring job {JobId} of type {JobType} with cron {Cron}",
            jobId,
            typeof(T).Name,
            cronExpression);
    }
}
