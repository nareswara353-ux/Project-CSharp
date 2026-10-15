using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Jobs;

public class AuditLogRetentionSettings
{
    public const string SectionName = "AuditLogRetention";

    public bool Enabled { get; set; } = true;

    [Range(1, 3650, ErrorMessage = "RetentionDays must be between 1 and 3650.")]
    public int RetentionDays { get; set; } = 365;

    [Range(100, 100000, ErrorMessage = "BatchSize must be between 100 and 100000.")]
    public int BatchSize { get; set; } = 5000;

    public bool ArchiveBeforeDelete { get; set; } = false;
}
