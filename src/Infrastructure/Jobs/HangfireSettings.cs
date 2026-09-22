using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Jobs;

public class HangfireSettings
{
    public const string SectionName = "Hangfire";

    public bool Enabled { get; set; }

    [Url(ErrorMessage = "Hangfire:DashboardPath must be a valid path.")]
    public string DashboardPath { get; set; } = "/hangfire";

    [Range(1, 100, ErrorMessage = "Hangfire:WorkerCount must be between 1 and 100.")]
    public int WorkerCount { get; set; } = 5;

    [Range(1, 3600, ErrorMessage = "Hangfire:QueuePollIntervalSeconds must be between 1 and 3600.")]
    public int QueuePollIntervalSeconds { get; set; } = 15;

    public bool EnableDashboard { get; set; } = true;
}
