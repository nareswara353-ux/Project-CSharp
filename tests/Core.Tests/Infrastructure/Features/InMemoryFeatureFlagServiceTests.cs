using Application.Common;
using FluentAssertions;
using Infrastructure.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace Core.Tests.Infrastructure.Features;

public class InMemoryFeatureFlagServiceTests
{
    private static InMemoryFeatureFlagService CreateService(
        Dictionary<string, string?>? initialFlags = null)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(initialFlags ?? new Dictionary<string, string?>())
            .Build();

        return new InMemoryFeatureFlagService(
            config,
            NullLogger<InMemoryFeatureFlagService>.Instance);
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldReturnDefault_WhenFlagMissing()
    {
        var service = CreateService();

        var result = await service.IsEnabledAsync("UnknownFlag", defaultValue: true);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldReturnFalse_ByDefaultWhenMissing()
    {
        var service = CreateService();

        var result = await service.IsEnabledAsync("UnknownFlag");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldReturnSeededValue_FromConfiguration()
    {
        var service = CreateService(new Dictionary<string, string?>
        {
            ["FeatureFlags:EnableNewCheckout"] = "true",
            ["FeatureFlags:MaintenanceMode"] = "false"
        });

        (await service.IsEnabledAsync("EnableNewCheckout")).Should().BeTrue();
        (await service.IsEnabledAsync("MaintenanceMode")).Should().BeFalse();
    }

    [Fact]
    public async Task IsEnabledAsync_ShouldBeCaseInsensitive()
    {
        var service = CreateService(new Dictionary<string, string?>
        {
            ["FeatureFlags:EnableNewCheckout"] = "true"
        });

        (await service.IsEnabledAsync("enablenewcheckout")).Should().BeTrue();
        (await service.IsEnabledAsync("ENABLENEWCHECKOUT")).Should().BeTrue();
    }

    [Fact]
    public async Task SetAsync_ShouldUpdateExistingFlag()
    {
        var service = CreateService();

        await service.SetAsync("MaintenanceMode", true);

        (await service.IsEnabledAsync("MaintenanceMode")).Should().BeTrue();
    }

    [Fact]
    public async Task SetAsync_ShouldCreateNewFlag_WhenNotExist()
    {
        var service = CreateService();

        await service.SetAsync("NewFlag", true);

        (await service.IsEnabledAsync("NewFlag")).Should().BeTrue();
    }

    [Fact]
    public async Task SetAsync_ShouldThrow_WhenFlagNameEmpty()
    {
        var service = CreateService();

        Func<Task> act = async () => await service.SetAsync("", true);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*cannot be empty*");
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllFlags()
    {
        var service = CreateService(new Dictionary<string, string?>
        {
            ["FeatureFlags:FlagA"] = "true",
            ["FeatureFlags:FlagB"] = "false",
            ["FeatureFlags:FlagC"] = "true"
        });

        var flags = await service.GetAllAsync();

        flags.Should().HaveCount(3);
        flags["FlagA"].Should().BeTrue();
        flags["FlagB"].Should().BeFalse();
        flags["FlagC"].Should().BeTrue();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnEmpty_WhenNoFlags()
    {
        var service = CreateService();

        var flags = await service.GetAllAsync();

        flags.Should().BeEmpty();
    }
}
