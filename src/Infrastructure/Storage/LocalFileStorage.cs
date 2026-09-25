using Application.Common;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

public class LocalFileStorage : IFileStorage
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<LocalFileStorage> _logger;
    private readonly string _rootPath;

    public LocalFileStorage(
        IOptions<FileStorageSettings> options,
        ILogger<LocalFileStorage> logger)
    {
        _settings = options.Value;
        _logger = logger;
        _rootPath = Path.Combine(_settings.BasePath, _settings.ContainerName);

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<StoredFile> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name cannot be empty", nameof(fileName));

        var extension = Path.GetExtension(fileName).ToLowerInvariant();

        if (_settings.AllowedExtensions.Length > 0 &&
            !_settings.AllowedExtensions.Contains(extension))
        {
            throw new InvalidOperationException(
                $"File extension '{extension}' is not allowed.");
        }

        var fileId = Guid.NewGuid().ToString("N") + extension;
        var fullPath = Path.Combine(_rootPath, fileId);

        long sizeBytes;

        await using (var fileStream = new FileStream(
            fullPath,
            FileMode.CreateNew,
            FileAccess.Write,
            FileShare.None,
            bufferSize: 4096,
            useAsync: true))
        {
            await content.CopyToAsync(fileStream, cancellationToken);
            sizeBytes = fileStream.Length;
        }

        var maxBytes = _settings.MaxSizeMb * 1024L * 1024L;
        if (sizeBytes > maxBytes)
        {
            File.Delete(fullPath);
            throw new InvalidOperationException(
                $"File exceeds maximum size of {_settings.MaxSizeMb} MB.");
        }

        _logger.LogInformation(
            "Uploaded file {FileId} ({Size} bytes) to local storage",
            fileId,
            sizeBytes);

        return new StoredFile(
            fileId,
            fileName,
            contentType,
            sizeBytes,
            DateTime.UtcNow);
    }

    public Task<Stream?> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(fileId);

        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);

        Stream stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            useAsync: true);

        return Task.FromResult<Stream?>(stream);
    }

    public Task<bool> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(fileId);

        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        _logger.LogInformation("Deleted file {FileId}", fileId);

        return Task.FromResult(true);
    }

    public Task<StoredFileInfo?> GetMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(fileId);

        if (!File.Exists(fullPath))
            return Task.FromResult<StoredFileInfo?>(null);

        var info = new System.IO.FileInfo(fullPath);

        return Task.FromResult<StoredFileInfo?>(new StoredFileInfo(
            fileId,
            Path.GetFileNameWithoutExtension(fileId),
            GetContentType(info.Extension),
            info.Length,
            info.CreationTimeUtc,
            FileStorageProviders.Local));
    }

    public Task<bool> ExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var fullPath = GetSafePath(fileId);
        return Task.FromResult(File.Exists(fullPath));
    }

    private string GetSafePath(string fileId)
    {
        if (string.IsNullOrWhiteSpace(fileId))
            throw new ArgumentException("File ID cannot be empty", nameof(fileId));

        var sanitized = Path.GetFileName(fileId);
        return Path.Combine(_rootPath, sanitized);
    }

    private static string GetContentType(string extension) => extension.ToLowerInvariant() switch
    {
        ".jpg" or ".jpeg" => "image/jpeg",
        ".png" => "image/png",
        ".gif" => "image/gif",
        ".webp" => "image/webp",
        ".pdf" => "application/pdf",
        ".txt" => "text/plain",
        ".csv" => "text/csv",
        ".json" => "application/json",
        _ => "application/octet-stream"
    };
}
