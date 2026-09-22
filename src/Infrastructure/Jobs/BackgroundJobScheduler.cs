using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Jobs;

public class BackgroundJobScheduler : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BackgroundJobScheduler> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public BackgroundJobScheduler(
        IServiceScopeFactory scopeFactory,
        ILogger<BackgroundJobScheduler> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("BackgroundJobScheduler started");

        await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);

        using var timer = new PeriodicTimer(_interval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunJobsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Scheduled job run failed");
            }

            try
            {
                if (!await timer.WaitForNextTickAsync(stoppingToken))
                    break;
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("BackgroundJobScheduler stopped");
    }

    private async Task RunJobsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var cleanupJob = scope.ServiceProvider.GetRequiredService<OrderCleanupJob>();
        var reportJob = scope.ServiceProvider.GetRequiredService<DailyReportJob>();

        await cleanupJob.ExecuteAsync();
        await reportJob.ExecuteAsync();

        _logger.LogInformation("Scheduled jobs completed at {Time}", DateTime.UtcNow);
    }
}
