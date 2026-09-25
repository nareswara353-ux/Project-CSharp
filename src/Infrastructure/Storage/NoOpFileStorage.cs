using Application.Common;

namespace Infrastructure.Storage;

public class NoOpFileStorage : IFileStorage
{
    private const string Message = "File storage is disabled. Configure FileStorage:Provider to enable.";

    public Task<StoredFile> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public Task<Stream?> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public Task<bool> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public Task<FileInfo?> GetMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);

    public Task<bool> ExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default)
        => throw new InvalidOperationException(Message);
}
