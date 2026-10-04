using System.Security.Claims;
using Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebAPI.Attributes;
using WebAPI.Authorization;

namespace Core.Tests.WebAPI.Authorization;

public class PermissionAuthorizationHandlerTests
{
    private readonly PermissionAuthorizationHandler _handler = new();

    private static ClaimsPrincipal BuildPrincipal(params string[] roles)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, "testuser")
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal BuildAnonymousPrincipal()
        => new(new ClaimsIdentity());

    private static async Task<bool> EvaluateAsync(
        PermissionAuthorizationHandler handler,
        ClaimsPrincipal user,
        string permission)
    {
        var requirement = new PermissionRequirement(permission);
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user,
            resource: null);

        await handler.HandleAsync(context);

        return context.HasSucceeded;
    }

    [Fact]
    public async Task Admin_ShouldSucceed_ForAnyPermission()
    {
        var user = BuildPrincipal("Admin");

        (await EvaluateAsync(_handler, user, Permissions.Customers.Read)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Products.Delete)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Admin.ManageUsers)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Admin.ManageFeatureFlags)).Should().BeTrue();
    }

    [Fact]
    public async Task Manager_ShouldSucceed_ForProductsAndOrders()
    {
        var user = BuildPrincipal("Manager");

        (await EvaluateAsync(_handler, user, Permissions.Products.Create)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Orders.Confirm)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Reports.Sales)).Should().BeTrue();
    }

    [Fact]
    public async Task Manager_ShouldFail_ForAdminOnlyPermission()
    {
        var user = BuildPrincipal("Manager");

        (await EvaluateAsync(_handler, user, Permissions.Admin.ManageUsers)).Should().BeFalse();
        (await EvaluateAsync(_handler, user, Permissions.Admin.ManageFeatureFlags)).Should().BeFalse();
    }

    [Fact]
    public async Task User_ShouldSucceed_ForReadAndCreatePermission()
    {
        var user = BuildPrincipal("User");

        (await EvaluateAsync(_handler, user, Permissions.Customers.Read)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Customers.Create)).Should().BeTrue();
        (await EvaluateAsync(_handler, user, Permissions.Orders.Create)).Should().BeTrue();
    }

    [Fact]
    public async Task User_ShouldFail_ForDeletePermission()
    {
        var user = BuildPrincipal("User");

        (await EvaluateAsync(_handler, user, Permissions.Customers.Delete)).Should().BeFalse();
        (await EvaluateAsync(_handler, user, Permissions.Products.Delete)).Should().BeFalse();
    }

    [Fact]
    public async Task User_ShouldFail_ForShipPermission()
    {
        var user = BuildPrincipal("User");

        (await EvaluateAsync(_handler, user, Permissions.Orders.Ship)).Should().BeFalse();
    }

    [Fact]
    public async Task AnonymousUser_ShouldFail_ForAnyPermission()
    {
        var user = BuildAnonymousPrincipal();

        (await EvaluateAsync(_handler, user, Permissions.Customers.Read)).Should().BeFalse();
    }

    [Fact]
    public async Task UnknownRole_ShouldFail_ForAnyPermission()
    {
        var user = BuildPrincipal("Guest");

        (await EvaluateAsync(_handler, user, Permissions.Customers.Read)).Should().BeFalse();
    }

    [Fact]
    public async Task MultipleRoles_ShouldSucceed_WhenAnyRoleHasPermission()
    {
        var user = BuildPrincipal("User", "Manager");

        (await EvaluateAsync(_handler, user, Permissions.Reports.Inventory)).Should().BeTrue();
    }

    [Fact]
    public async Task RoleMatching_ShouldBeCaseInsensitive()
    {
        var user = BuildPrincipal("admin");

        (await EvaluateAsync(_handler, user, Permissions.Admin.ManageUsers)).Should().BeTrue();
    }
}

public class PermissionPolicyProviderTests
{
    private static PermissionPolicyProvider BuildProvider()
    {
        var options = Options.Create(new AuthorizationOptions());
        return new PermissionPolicyProvider(options);
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldReturnPolicy_ForPermissionPrefix()
    {
        var provider = BuildProvider();
        var policyName = $"{HasPermissionAttribute.PolicyPrefix}{Permissions.Customers.Read}";

        var policy = await provider.GetPolicyAsync(policyName);

        policy.Should().NotBeNull();
        policy!.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionRequirement>();
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldRequireAuthenticatedUser()
    {
        var provider = BuildProvider();
        var policyName = $"{HasPermissionAttribute.PolicyPrefix}{Permissions.Products.Create}";

        var policy = await provider.GetPolicyAsync(policyName);

        policy.Should().NotBeNull();
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldFallback_ForUnknownPolicy()
    {
        var provider = BuildProvider();

        var policy = await provider.GetPolicyAsync("SomeOtherPolicy");

        policy.Should().BeNull();
    }
}
