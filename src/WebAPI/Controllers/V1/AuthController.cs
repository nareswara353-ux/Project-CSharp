using Application.Auth;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route(ApiRoutes.Auth)]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return StatusCode(StatusCodes.Status201Created, result.Value);

        return result.ErrorCode switch
        {
            "USERNAME_TAKEN" or "EMAIL_TAKEN" => Conflict(new { error = result.Error, code = result.ErrorCode }),
            _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
            return Ok(result.Value);

        return result.ErrorCode switch
        {
            "INVALID_CREDENTIALS" or "USER_INACTIVE" => Unauthorized(new { error = result.Error, code = result.ErrorCode }),
            _ => BadRequest(new { error = result.Error, code = result.ErrorCode })
        };
    }
}
