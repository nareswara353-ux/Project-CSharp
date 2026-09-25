using Domain.Common;

namespace Domain.ValueObjects;

public sealed class FileMetadata : ValueObject
{
    public const long MaxSizeBytes = 10 * 1024 * 1024;
    public const int MaxFileNameLength = 255;

    public string FileName { get; } = null!;
    public string ContentType { get; } = null!;
    public long SizeBytes { get; }

    private FileMetadata() { }

    public FileMetadata(string fileName, string contentType, long sizeBytes)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        if (fileName.Length > MaxFileNameLength)
            throw new ArgumentException($"File name cannot exceed {MaxFileNameLength} characters", nameof(fileName));

        if (string.IsNullOrWhiteSpace(contentType))
            throw new ArgumentException("Content type cannot be empty", nameof(contentType));

        if (sizeBytes <= 0)
            throw new ArgumentException("File size must be positive", nameof(sizeBytes));

        if (sizeBytes > MaxSizeBytes)
            throw new ArgumentException($"File size cannot exceed {MaxSizeBytes / 1024 / 1024} MB", nameof(sizeBytes));

        FileName = SanitizeFileName(fileName);
        ContentType = contentType.ToLowerInvariant().Trim();
        SizeBytes = sizeBytes;
    }

    public string Extension => Path.GetExtension(FileName).ToLowerInvariant();

    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    public bool IsDocument => ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase)
        || ContentType.Contains("word", StringComparison.OrdinalIgnoreCase)
        || ContentType.Contains("excel", StringComparison.OrdinalIgnoreCase)
        || ContentType.Contains("text", StringComparison.OrdinalIgnoreCase);

    private static string SanitizeFileName(string fileName)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var sanitized = new string(fileName
            .Where(c => !invalid.Contains(c))
            .ToArray());

        return sanitized.Trim();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FileName;
        yield return ContentType;
        yield return SizeBytes;
    }

    public override string ToString() => $"{FileName} ({ContentType}, {SizeBytes} bytes)";
}
