using Application.Common;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Storage;

public class AzureBlobFileStorage : IFileStorage
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<AzureBlobFileStorage> _logger;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobFileStorage(
        IOptions<FileStorageSettings> options,
        ILogger<AzureBlobFileStorage> logger)
    {
        _settings = options.Value;
        _logger = logger;

        if (string.IsNullOrWhiteSpace(_settings.AzureConnectionString))
            throw new InvalidOperationException(
                "FileStorage:AzureConnectionString is required when Provider is AzureBlob.");

        var serviceClient = new BlobServiceClient(_settings.AzureConnectionString);
        _containerClient = serviceClient.GetBlobContainerClient(_settings.ContainerName);

        _containerClient.CreateIfNotExists(
            _settings.PublicAccess
                ? PublicAccessType.Blob
                : PublicAccessType.None);
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
        var blobClient = _containerClient.GetBlobClient(fileId);

        var blobHttpHeaders = new BlobHttpHeaders
        {
            ContentType = contentType
        };

        var blobOptions = new BlobUploadOptions
        {
            HttpHeaders = blobHttpHeaders,
            Metadata = new Dictionary<string, string>
            {
                ["originalFileName"] = fileName,
                ["uploadedAt"] = DateTime.UtcNow.ToString("O")
            }
        };

        await blobClient.UploadAsync(content, blobOptions, cancellationToken);

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Uploaded file {FileId} ({Size} bytes) to Azure Blob",
            fileId,
            properties.Value.ContentLength);

        return new StoredFile(
            fileId,
            fileName,
            contentType,
            properties.Value.ContentLength,
            DateTime.UtcNow);
    }

    public async Task<Stream?> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var response = await blobClient.DownloadStreamingAsync(
            cancellationToken: cancellationToken);

        return response.Value.Content;
    }

    public async Task<bool> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);

        var response = await blobClient.DeleteIfExistsAsync(
            DeleteSnapshotsOption.IncludeSnapshots,
            cancellationToken: cancellationToken);

        if (response.Value)
            _logger.LogInformation("Deleted blob {FileId}", fileId);

        return response.Value;
    }

    public async Task<StoredFileInfo?> GetMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);

        if (!await blobClient.ExistsAsync(cancellationToken))
            return null;

        var properties = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);

        var originalName = properties.Value.Metadata.TryGetValue("originalFileName", out var name)
            ? name
            : fileId;

        return new StoredFileInfo(
            fileId,
            originalName,
            properties.Value.ContentType,
            properties.Value.ContentLength,
            properties.Value.CreatedOn.UtcDateTime,
            FileStorageProviders.AzureBlob);
    }

    public async Task<bool> ExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default)
    {
        var blobClient = _containerClient.GetBlobClient(fileId);
        var response = await blobClient.ExistsAsync(cancellationToken);
        return response.Value;
    }
}
