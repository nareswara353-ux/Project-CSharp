using Application.Common;
using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class AuditLoggingBehaviorTests
{
    private readonly Mock<IAuditService> _auditService = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private AuditLoggingBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>()
        where TRequest : notnull
        => new(
            _auditService.Object,
            _currentUser.Object,
            NullLogger<AuditLoggingBehavior<TRequest, TResponse>>.Instance);

    public record AuditableRequest(string Data) : IRequest<string>, IAuditableRequest
    {
        public string AuditAction => "TestAction";
        public string AuditEntityType => "TestEntity";
        public string? AuditEntityId => "test-id";
    }

    public record NonAuditableRequest(string Data) : IRequest<string>;

    private void SetupCurrentUser()
    {
        _currentUser.SetupGet(u => u.UserId).Returns(Guid.NewGuid());
        _currentUser.SetupGet(u => u.Username).Returns("testuser");
        _currentUser.SetupGet(u => u.CorrelationId).Returns("corr-123");
        _currentUser.SetupGet(u => u.IpAddress).Returns("127.0.0.1");
    }

    [Fact]
    public async Task Handle_ShouldLogSuccess_WhenRequestAuditable()
    {
        SetupCurrentUser();

        _auditService
            .Setup(s => s.LogAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var behavior = CreateBehavior<AuditableRequest, string>();
        var request = new AuditableRequest("payload");

        var response = await behavior.Handle(
            request,
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");

        _auditService.Verify(s => s.LogAsync(
            It.Is<AuditEntry>(e =>
                e.Action == "TestAction" &&
                e.EntityType == "TestEntity" &&
                e.EntityId == "test-id" &&
                e.Success == true &&
                e.Username == "testuser" &&
                e.CorrelationId == "corr-123" &&
                e.ErrorMessage == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldLogFailure_WhenRequestThrows()
    {
        SetupCurrentUser();

        _auditService
            .Setup(s => s.LogAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var behavior = CreateBehavior<AuditableRequest, string>();
        var request = new AuditableRequest("payload");

        Func<Task> act = async () => await behavior.Handle(
            request,
            _ => throw new InvalidOperationException("boom"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("boom");

        _auditService.Verify(s => s.LogAsync(
            It.Is<AuditEntry>(e =>
                e.Success == false &&
                e.ErrorMessage == "boom" &&
                e.Action == "TestAction"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotLog_WhenRequestNotAuditable()
    {
        var behavior = CreateBehavior<NonAuditableRequest, string>();
        var request = new NonAuditableRequest("payload");

        await behavior.Handle(
            request,
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        _auditService.Verify(s => s.LogAsync(
            It.IsAny<AuditEntry>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenAuditServiceFails()
    {
        SetupCurrentUser();

        _auditService
            .Setup(s => s.LogAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("audit down"));

        var behavior = CreateBehavior<AuditableRequest, string>();
        var request = new AuditableRequest("payload");

        Func<Task> act = async () => await behavior.Handle(
            request,
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        await act.Should().NotThrowAsync();
    }
}
