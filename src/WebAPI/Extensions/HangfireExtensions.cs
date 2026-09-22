using Hangfire;
using Infrastructure.Jobs;

namespace WebAPI.Extensions;

public static class HangfireExtensions
{
    public static IApplicationBuilder UseHangfireDashboardIfEnabled(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        var settings = configuration
            .GetSection(HangfireSettings.SectionName)
            .Get<HangfireSettings>();

        if (settings is null || !settings.Enabled || !settings.EnableDashboard)
            return app;

        app.UseHangfireDashboard(settings.DashboardPath, new DashboardOptions
        {
            DashboardTitle = "Portfolio Enterprise Jobs",
            StatsPollingInterval = 5000
        });

        return app;
    }

    public static void RegisterHangfireRecurringJobs(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var manager = scope.ServiceProvider.GetService<IRecurringJobManager>();
        var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();

        if (manager is null)
            return;

        HangfireJobRegistration.RegisterRecurringJobs(
            manager,
            loggerFactory.CreateLogger("HangfireJobRegistration"));
    }
}
