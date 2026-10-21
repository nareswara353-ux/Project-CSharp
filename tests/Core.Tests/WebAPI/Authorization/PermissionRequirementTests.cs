using Application.Common;
using FluentAssertions;
using WebAPI.Authorization;

namespace Core.Tests.WebAPI.Authorization;

public class PermissionRequirementTests
{
    [Fact]
    public void Constructor_ShouldSetPermission()
    {
        var requirement = new PermissionRequirement(Permissions.Customers.Read);

        requirement.Permission.Should().Be(Permissions.Customers.Read);
    }

    [Fact]
    public void Constructor_ShouldAcceptAnyString()
    {
        var requirement = new PermissionRequirement("custom.permission.here");

        requirement.Permission.Should().Be("custom.permission.here");
    }

    [Theory]
    [InlineData(Permissions.Customers.Read)]
    [InlineData(Permissions.Customers.Create)]
    [InlineData(Permissions.Products.Update)]
    [InlineData(Permissions.Orders.Ship)]
    [InlineData(Permissions.Reports.Sales)]
    [InlineData(Permissions.Admin.ManageUsers)]
    public void Constructor_ShouldStoreKnownPermissions(string permission)
    {
        var requirement = new PermissionRequirement(permission);

        requirement.Permission.Should().Be(permission);
    }

    [Fact]
    public void Permissions_All_ShouldContainAllDeclaredConstants()
    {
        Permissions.All.Should().Contain(new[]
        {
            Permissions.Customers.Read,
            Permissions.Customers.Create,
            Permissions.Customers.Update,
            Permissions.Customers.Delete,
            Permissions.Products.Read,
            Permissions.Products.Create,
            Permissions.Products.Update,
            Permissions.Products.Delete,
            Permissions.Orders.Read,
            Permissions.Orders.Create,
            Permissions.Orders.Confirm,
            Permissions.Orders.Ship,
            Permissions.Orders.Cancel,
            Permissions.Reports.Sales,
            Permissions.Reports.Inventory,
            Permissions.Admin.ManageUsers,
            Permissions.Admin.ManageFeatureFlags,
            Permissions.Admin.ViewAuditLog
        });
    }
}
