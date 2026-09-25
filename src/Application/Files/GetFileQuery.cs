using Application.Common;
using FluentValidation;
using MediatR;

namespace Application.Files;

public record GetFileQuery : IRequest<Result<FileDownload>>
{
    public string FileId { get; init; } = string.Empty;
}

public record FileDownload(Stream Content, FileInfo Metadata);

public class GetFileQueryHandler : IRequestHandler<GetFileQuery, Result<FileDownload>>
{
    private readonly IFileStorage _fileStorage;

    public GetFileQueryHandler(IFileStorage fileStorage)
    {
        _fileStorage = fileStorage;
    }

    public async Task<Result<FileDownload>> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var metadata = await _fileStorage.GetMetadataAsync(request.FileId, cancellationToken);
            if (metadata is null)
                return Result<FileDownload>.Failure($"File '{request.FileId}' not found", "NOT_FOUND");

            var stream = await _fileStorage.DownloadAsync(request.FileId, cancellationToken);
            if (stream is null)
                return Result<FileDownload>.Failure($"File '{request.FileId}' content unavailable", "NOT_FOUND");

            return Result<FileDownload>.Success(new FileDownload(stream, metadata));
        }
        catch (Exception ex)
        {
            return Result<FileDownload>.Failure($"Failed to retrieve file: {ex.Message}", "GET_FAILED");
        }
    }
}

public class GetFileQueryValidator : AbstractValidator<GetFileQuery>
{
    public GetFileQueryValidator()
    {
        RuleFor(x => x.FileId)
            .NotEmpty().WithMessage("File ID is required");
    }
}
