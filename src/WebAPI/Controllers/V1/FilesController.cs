using Application.Common;
using Application.Files;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/files")]
[Authorize]
public class FilesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FilesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    [ProducesResponseType(typeof(StoredFile), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file is null || file.Length == 0)
            return BadRequest(new { error = "File is required", code = "FILE_REQUIRED" });

        await using var stream = file.OpenReadStream();

        var command = new UploadFileCommand
        {
            Content = stream,
            FileName = file.FileName,
            ContentType = file.ContentType
        };

        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => StatusCode(StatusCodes.Status201Created, result.Value),
            onFailure: () => BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Download(string id)
    {
        var result = await _mediator.Send(new GetFileQuery { FileId = id });

        if (result.IsFailure)
        {
            return result.ErrorCode == "NOT_FOUND"
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error, code = result.ErrorCode });
        }

        var download = result.Value!;

        return File(
            download.Content,
            download.Metadata.ContentType,
            download.Metadata.FileName);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await _mediator.Send(new DeleteFileCommand { FileId = id });

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode == "NOT_FOUND"
                ? NotFound(new { error = result.Error })
                : BadRequest(new { error = result.Error, code = result.ErrorCode }));
    }
}
