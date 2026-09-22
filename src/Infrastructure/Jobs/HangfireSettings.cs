namespace Infrastructure.Jobs;

public class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public bool Enabled { get; set; } = false;
    public string? ConnectionString { get; set; }
    public string DashboardPath { get; set; } = "/hangfire";
    public int WorkerCount { get; set; } = 5;
    public int QueuePollIntervalSeconds { get; set; } = 15;
    public bool EnableDashboard { get; set; } = true;
}
