using Application.Common;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using WebAPI.Attributes;
using WebAPI.Authorization;

namespace Core.Tests.WebAPI.Authorization;

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
        policy!.Requirements.Should().Contain(r => r is PermissionRequirement);
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldIncludePermissionRequirement()
    {
        var provider = BuildProvider();
        var policyName = $"{HasPermissionAttribute.PolicyPrefix}{Permissions.Products.Create}";

        var policy = await provider.GetPolicyAsync(policyName);

        var permissionReq = policy!.Requirements
            .OfType<PermissionRequirement>()
            .SingleOrDefault();

        permissionReq.Should().NotBeNull();
        permissionReq!.Permission.Should().Be(Permissions.Products.Create);
    }

    [Fact]
    public async Task GetPolicyAsync_ShouldFallback_ForUnknownPolicy()
    {
        var provider = BuildProvider();

        var policy = await provider.GetPolicyAsync("SomeOtherPolicy");

        policy.Should().BeNull();
    }
}
