using Application.Common;
using FluentValidation;
using MediatR;

namespace Application.Files;

public record UploadFileCommand : IRequest<Result<StoredFile>>
{
    public Stream Content { get; init; } = Stream.Null;
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = string.Empty;
}

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<StoredFile>>
{
    private readonly IFileStorage _fileStorage;

    public UploadFileCommandHandler(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<Result<StoredFile>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var stored = await _fileStorage.UploadAsync(
                request.Content,
                request.FileName,
                request.ContentType,
                cancellationToken);

            return Result<StoredFile>.Success(stored);
        }
        catch (InvalidOperationException ex)
        {
            return Result<StoredFile>.Failure(ex.Message, "UPLOAD_REJECTED");
        }
        catch (ArgumentException ex)
        {
            return Result<StoredFile>.Failure($"Validation error: {ex.Message}", "VALIDATION_ERROR");
        }
        catch (Exception ex)
        {
            return Result<StoredFile>.Failure($"Failed to upload file: {ex.Message}", "UPLOAD_FAILED");
        }
    }
}

public class UploadFileCommandValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileCommandValidator()
    {
        RuleFor(x => x.FileName)
            .NotEmpty().WithMessage("File name is required")
            .MaximumLength(255).WithMessage("File name must not exceed 255 characters");

        RuleFor(x => x.ContentType)
            .NotEmpty().WithMessage("Content type is required");

        RuleFor(x => x.Content)
            .NotNull().WithMessage("File content is required");
    }
}
