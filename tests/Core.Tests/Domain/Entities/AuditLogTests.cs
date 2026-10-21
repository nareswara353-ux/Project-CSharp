using Domain.Entities;
using FluentAssertions;

namespace Core.Tests.Domain.Entities;

public class AuditLogTests
{
    [Fact]
    public void Constructor_ShouldCreateAuditLog_WhenValidParameters()
    {
        var userId = Guid.NewGuid();

        var log = new AuditLog(
            action: "CustomerCreated",
            entityType: "Customer",
            success: true,
            userId: userId,
            username: "admin",
            entityId: "123",
            payload: "{\"name\":\"John\"}",
            ipAddress: "192.168.1.1",
            userAgent: "Mozilla/5.0",
            correlationId: "corr-123",
            durationMs: 250);

        log.Action.Should().Be("CustomerCreated");
        log.EntityType.Should().Be("Customer");
        log.Success.Should().BeTrue();
        log.UserId.Should().Be(userId);
        log.Username.Should().Be("admin");
        log.EntityId.Should().Be("123");
        log.Payload.Should().Contain("John");
        log.IpAddress.Should().Be("192.168.1.1");
        log.CorrelationId.Should().Be("corr-123");
        log.DurationMs.Should().Be(250);
        log.OccurredOn.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenActionInvalid(string? action)
    {
        Action act = () => new AuditLog(
            action: action!,
            entityType: "Customer",
            success: true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Action cannot be empty*");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenEntityTypeInvalid(string? entityType)
    {
        Action act = () => new AuditLog(
            action: "CustomerCreated",
            entityType: entityType!,
            success: true);

        act.Should().Throw<ArgumentException>()
            .WithMessage("*Entity type cannot be empty*");
    }

    [Fact]
    public void Constructor_ShouldTrimActionAndEntityType()
    {
        var log = new AuditLog(
            action: "  CustomerCreated  ",
            entityType: "  Customer  ",
            success: true);

        log.Action.Should().Be("CustomerCreated");
        log.EntityType.Should().Be("Customer");
    }

    [Fact]
    public void Constructor_ShouldAllowNullOptionalFields()
    {
        var log = new AuditLog(
            action: "TestAction",
            entityType: "Test",
            success: false);

        log.UserId.Should().BeNull();
        log.Username.Should().BeNull();
        log.EntityId.Should().BeNull();
        log.Payload.Should().BeNull();
        log.ErrorMessage.Should().BeNull();
        log.IpAddress.Should().BeNull();
        log.UserAgent.Should().BeNull();
        log.CorrelationId.Should().BeNull();
        log.DurationMs.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldSetSuccessFalse_WithErrorMessage()
    {
        var log = new AuditLog(
            action: "CustomerCreated",
            entityType: "Customer",
            success: false,
            errorMessage: "Database failure");

        log.Success.Should().BeFalse();
        log.ErrorMessage.Should().Be("Database failure");
    }
}
