using Application.Common;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Common;

namespace WebAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/featureflags")]
[Authorize]
public class FeatureFlagsController : ControllerBase
{
    private readonly IFeatureFlagService _featureFlags;
    private readonly ILogger<FeatureFlagsController> _logger;

    public FeatureFlagsController(
        IFeatureFlagService featureFlags,
        ILogger<FeatureFlagsController> logger)
    {
        _featureFlags = featureFlags;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyDictionary<string, bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var flags = await _featureFlags.GetAllAsync();
        return Ok(flags);
    }

    [HttpGet("{name}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(string name)
    {
        var enabled = await _featureFlags.IsEnabledAsync(name, defaultValue: false);
        return Ok(new { name, enabled });
    }

    [HttpPut("{name}")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Set(string name, [FromBody] SetFeatureFlagRequest request)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Flag name is required", code = "INVALID_FLAG_NAME" });

        await _featureFlags.SetAsync(name, request.Enabled);

        _logger.LogInformation(
            "Feature flag {FlagName} updated to {Enabled} by {User}",
            name,
            request.Enabled,
            User.Identity?.Name ?? "unknown");

        return NoContent();
    }
}

public record SetFeatureFlagRequest(bool Enabled);
