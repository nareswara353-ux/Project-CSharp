using Application.Common;
using FluentValidation;
using MediatR;

namespace Application.Files;

public record DeleteFileCommand : IRequest<Result>
{
    public string FileId { get; init; } = string.Empty;
}

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IFileStorage _fileStorage;

    public DeleteFileCommandHandler(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var deleted = await _fileStorage.DeleteAsync(request.FileId, cancellationToken);

            if (!deleted)
                return Result.Failure($"File '{request.FileId}' not found", "NOT_FOUND");

            return Result.Success();
        }
        catch (Exception ex)
        {
            return Result.Failure($"Failed to delete file: {ex.Message}", "DELETE_FAILED");
        }
    }
}

public class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required");
    }
}
