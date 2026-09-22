using System.Security.Claims;
using Application.Users;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.Users)]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;

    public UsersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new { error = "Invalid token", code = "INVALID_TOKEN" });

        var result = await _mediator.Send(new GetCurrentUserQuery { UserId = userId.Value });

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpGet("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetUserByIdQuery { Id = id });

        return result.Match(
            onSuccess: () => Ok(result.Value),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPost("me/change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
            return Unauthorized(new { error = "Invalid token", code = "INVALID_TOKEN" });

        var actualCommand = command with { UserId = userId.Value };
        var result = await _mediator.Send(actualCommand);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    [HttpPut("{id:guid}/role")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateUserRoleRequest request)
    {
        var command = new UpdateUserRoleCommand { UserId = id, Role = request.Role };
        var result = await _mediator.Send(command);

        return result.Match(
            onSuccess: () => NoContent(),
            onFailure: () => result.ErrorCode switch
            {
                "NOT_FOUND" => NotFound(new { error = result.Error }),
                _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
            });
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var id) ? id : null;
    }
}

public record UpdateUserRoleRequest(string Role);
