using System.Text;
using FluentAssertions;
using Infrastructure.Storage;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Core.Tests.Infrastructure.Storage;

public class LocalFileStorageTests : IDisposable
{
    private readonly string _tempRoot;
    private readonly LocalFileStorage _storage;

    public LocalFileStorageTests()
    {
        _tempRoot = Path.Combine(Path.GetTempPath(), $"portfolio-test-{Guid.NewGuid():N}");

        var settings = new FileStorageSettings
        {
            Provider = FileStorageProviders.Local,
            BasePath = _tempRoot,
            ContainerName = "uploads",
            MaxSizeMb = 1,
            AllowedExtensions = new[] { ".txt", ".pdf", ".jpg" }
        };

        var options = Options.Create(settings);
        _storage = new LocalFileStorage(options, NullLogger<LocalFileStorage>.Instance);
    }

    private static Stream CreateStream(string content)
        => new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task UploadAsync_ShouldStoreFile_WhenValid()
    {
        var stored = await _storage.UploadAsync(
            CreateStream("hello world"),
            "greeting.txt",
            "text/plain");

        stored.FileName.Should().Be("greeting.txt");
        stored.ContentType.Should().Be("text/plain");
        stored.SizeBytes.Should().BeGreaterThan(0);
        stored.FileId.Should().EndWith(".txt");
    }

    [Fact]
    public async Task UploadAsync_ShouldReject_WhenExtensionNotAllowed()
    {
        Func<Task> act = async () => await _storage.UploadAsync(
            CreateStream("binary"),
            "malware.exe",
            "application/octet-stream");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not allowed*");
    }

    [Fact]
    public async Task UploadAsync_ShouldReject_WhenFileTooLarge()
    {
        var largeContent = new byte[2 * 1024 * 1024];
        var stream = new MemoryStream(largeContent);

        Func<Task> act = async () => await _storage.UploadAsync(
            stream,
            "big.txt",
            "text/plain");

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*exceeds maximum size*");
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnContent_WhenFileExists()
    {
        var stored = await _storage.UploadAsync(
            CreateStream("download me"),
            "test.txt",
            "text/plain");

        var stream = await _storage.DownloadAsync(stored.FileId);
        stream.Should().NotBeNull();

        using var reader = new StreamReader(stream!);
        var content = await reader.ReadToEndAsync();

        content.Should().Be("download me");
        stream!.Dispose();
    }

    [Fact]
    public async Task DownloadAsync_ShouldReturnNull_WhenFileMissing()
    {
        var stream = await _storage.DownloadAsync("nonexistent.txt");
        stream.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveFile_WhenFileExists()
    {
        var stored = await _storage.UploadAsync(
            CreateStream("to delete"),
            "delete-me.txt",
            "text/plain");

        var deleted = await _storage.DeleteAsync(stored.FileId);

        deleted.Should().BeTrue();
        (await _storage.ExistsAsync(stored.FileId)).Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenFileMissing()
    {
        var deleted = await _storage.DeleteAsync("nonexistent.txt");
        deleted.Should().BeFalse();
    }

    [Fact]
    public async Task ExistsAsync_ShouldReturnTrue_WhenFileExists()
    {
        var stored = await _storage.UploadAsync(
            CreateStream("exists"),
            "exists.txt",
            "text/plain");

        (await _storage.ExistsAsync(stored.FileId)).Should().BeTrue();
    }

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnInfo_WhenFileExists()
    {
        var stored = await _storage.UploadAsync(
            CreateStream("metadata check"),
            "meta.txt",
            "text/plain");

        var info = await _storage.GetMetadataAsync(stored.FileId);

        info.Should().NotBeNull();
        info!.FileId.Should().Be(stored.FileId);
        info.ContentType.Should().Be("text/plain");
        info.StorageProvider.Should().Be(FileStorageProviders.Local);
    }

    [Fact]
    public async Task GetMetadataAsync_ShouldReturnNull_WhenFileMissing()
    {
        var info = await _storage.GetMetadataAsync("missing.txt");
        info.Should().BeNull();
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempRoot))
            Directory.Delete(_tempRoot, recursive: true);
    }
}
