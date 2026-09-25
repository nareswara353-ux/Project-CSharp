namespace Application.Common;

public interface IFileStorage
{
    Task<StoredFile> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream?> DownloadAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<FileInfo?> GetMetadataAsync(
        string fileId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string fileId,
        CancellationToken cancellationToken = default);
}

public record StoredFile(
    string FileId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt);

public record FileInfo(
    string FileId,
    string FileName,
    string ContentType,
    long SizeBytes,
    DateTime UploadedAt,
    string StorageProvider);
