using System.Security.Claims;

namespace WebAPI.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;

        return Guid.TryParse(claim, out var id) ? id : null;
    }

    public static string? GetUsername(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("unique_name")?.Value;

    public static string? GetEmail(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;

    public static string? GetRole(this ClaimsPrincipal principal)
        => principal.FindFirst(ClaimTypes.Role)?.Value;

    public static bool IsAdmin(this ClaimsPrincipal principal)
        => principal.IsInRole("Admin");

    public static bool IsManager(this ClaimsPrincipal principal)
        => principal.IsInRole("Manager");
}
