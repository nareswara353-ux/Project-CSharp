using System.Security.Claims;
using Application.Common;
using Microsoft.AspNetCore.Authorization;
using WebAPI.Attributes;

namespace WebAPI.Authorization;

public sealed class PermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public PermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private static readonly Dictionary<string, HashSet<string>> RolePermissions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Admin"] = new(Permissions.All, StringComparer.OrdinalIgnoreCase),

        ["Manager"] = new(StringComparer.OrdinalIgnoreCase)
        {
            Permissions.Customers.Read, Permissions.Customers.Create,
            Permissions.Customers.Update, Permissions.Customers.Delete,
            Permissions.Products.Read, Permissions.Products.Create,
            Permissions.Products.Update, Permissions.Products.Delete,
            Permissions.Orders.Read, Permissions.Orders.Create,
            Permissions.Orders.Confirm, Permissions.Orders.Ship,
            Permissions.Orders.Cancel,
            Permissions.Reports.Sales, Permissions.Reports.Inventory
        },

        ["User"] = new(StringComparer.OrdinalIgnoreCase)
        {
            Permissions.Customers.Read, Permissions.Customers.Create,
            Permissions.Customers.Update,
            Permissions.Products.Read,
            Permissions.Orders.Read, Permissions.Orders.Create,
            Permissions.Orders.Confirm, Permissions.Orders.Cancel
        }
    };

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
            return Task.CompletedTask;

        var roles = context.User
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        foreach (var role in roles)
        {
            if (RolePermissions.TryGetValue(role, out var permissions) &&
                permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }
        }

        return Task.CompletedTask;
    }
}

public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(HasPermissionAttribute.PolicyPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permission = policyName.Substring(HasPermissionAttribute.PolicyPrefix.Length);

            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(permission))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        return _fallback.GetPolicyAsync(policyName);
    }
}
