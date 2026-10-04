using Application.Common;
using Application.Users;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Attributes;
using WebAPI.Common;
using WebAPI.Extensions;

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
        var userId = HttpContext.GetUserId();
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
    [HasPermission(Permissions.Admin.ManageUsers)]
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
        var userId = HttpContext.GetUserId();
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
    [HasPermission(Permissions.Admin.ManageUsers)]
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
}

public record UpdateUserRoleRequest(string Role);
