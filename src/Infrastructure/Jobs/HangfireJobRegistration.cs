using Application.Common;
using Hangfire;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public static class HangfireJobRegistration
{
    public static void RegisterRecurringJobs(IRecurringJobManager manager, ILogger logger)
    {
        manager.AddOrUpdate<OrderCleanupJob>(
            JobIds.OrderCleanup,
            job => job.ExecuteAsync(),
            Cron.Daily(2));

        manager.AddOrUpdate<DailyReportJob>(
            JobIds.DailyReport,
            job => job.ExecuteAsync(),
            Cron.Daily(6));

        logger.LogInformation("Hangfire recurring jobs registered");
    }
}
