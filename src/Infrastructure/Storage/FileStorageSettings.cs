using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Storage;

public class FileStorageSettings
{
    public const string SectionName = "FileStorage";

    [Required]
    public string Provider { get; set; } = FileStorageProviders.Local;

    public string BasePath { get; set; } = "storage";

    public string ContainerName { get; set; } = "uploads";

    [Range(1, 100, ErrorMessage = "MaxSizeMb must be between 1 and 100.")]
    public int MaxSizeMb { get; set; } = 10;

    public string[] AllowedExtensions { get; set; } = new[]
    {
        ".jpg", ".jpeg", ".png", ".gif", ".webp",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx",
        ".txt", ".csv"
    };

    public string? AzureConnectionString { get; set; }

    public bool PublicAccess { get; set; } = false;
}

public static class FileStorageProviders
{
    public const string None = "None";
    public const string Local = "Local";
    public const string AzureBlob = "AzureBlob";
}
