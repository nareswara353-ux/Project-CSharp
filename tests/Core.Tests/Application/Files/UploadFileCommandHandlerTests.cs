using System.Text;
using Application.Common;
using Application.Files;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Files;

public class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileStorage> _storage = new();
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _handler = new UploadFileCommandHandler(_storage.Object);
    }

    private static Stream CreateStream(string content = "test content")
        => new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenUploadValid()
    {
        var stored = new StoredFile(
            "abc123.txt",
            "greeting.txt",
            "text/plain",
            12,
            DateTime.UtcNow);

        _storage
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stored);

        var command = new UploadFileCommand
        {
            Content = CreateStream(),
            FileName = "greeting.txt",
            ContentType = "text/plain"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value!.FileId.Should().Be("abc123.txt");
        result.Value.FileName.Should().Be("greeting.txt");
        _storage.Verify(s => s.UploadAsync(
            It.IsAny<Stream>(),
            "greeting.txt",
            "text/plain",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenExtensionNotAllowed()
    {
        _storage
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("File extension '.exe' is not allowed."));

        var command = new UploadFileCommand
        {
            Content = CreateStream(),
            FileName = "malware.exe",
            ContentType = "application/octet-stream"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("UPLOAD_REJECTED");
        result.Error.Should().Contain("not allowed");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenFileTooLarge()
    {
        _storage
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("File exceeds maximum size of 10 MB."));

        var command = new UploadFileCommand
        {
            Content = CreateStream(),
            FileName = "huge.pdf",
            ContentType = "application/pdf"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("UPLOAD_REJECTED");
        result.Error.Should().Contain("maximum size");
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenArgumentInvalid()
    {
        _storage
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("File name cannot be empty"));

        var command = new UploadFileCommand
        {
            Content = CreateStream(),
            FileName = "test.txt",
            ContentType = "text/plain"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenStorageThrowsGenericException()
    {
        _storage
            .Setup(s => s.UploadAsync(
                It.IsAny<Stream>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Unexpected storage failure"));

        var command = new UploadFileCommand
        {
            Content = CreateStream(),
            FileName = "test.txt",
            ContentType = "text/plain"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("UPLOAD_FAILED");
    }
}
