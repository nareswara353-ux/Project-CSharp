using System.Linq.Expressions;
using Application.Auditing;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Auditing;

public class GetAuditLogsQueryHandlerTests
{
    private readonly Mock<IAuditLogRepository> _repo = new();
    private readonly GetAuditLogsQueryHandler _handler;

    public GetAuditLogsQueryHandlerTests()
    {
        _handler = new GetAuditLogsQueryHandler(_repo.Object);
    }

    private static AuditLog BuildLog(string action, string entityType, bool success = true)
        => new(
            action: action,
            entityType: entityType,
            success: success,
            userId: Guid.NewGuid(),
            username: "testuser");

    private void SetupPagedReturn(IReadOnlyList<AuditLog> items, int totalCount)
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(items);

        _repo.Setup(r => r.CountAsync(
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(totalCount);
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenLogsExist()
    {
        var logs = new List<AuditLog>
        {
            BuildLog("CustomerCreated", "Customer"),
            BuildLog("OrderCreated", "Order")
        };

        SetupPagedReturn(logs, 2);

        var result = await _handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(50);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoLogs()
    {
        SetupPagedReturn(new List<AuditLog>(), 0);

        var result = await _handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldRespectPagination()
    {
        SetupPagedReturn(new List<AuditLog> { BuildLog("CustomerCreated", "Customer") }, 100);

        var query = new GetAuditLogsQuery { PageNumber = 2, PageSize = 10 };
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PageNumber.Should().Be(2);
        result.Value.PageSize.Should().Be(10);

        _repo.Verify(r => r.GetPagedAsync(
            2, 10,
            It.IsAny<Expression<Func<AuditLog, bool>>>(),
            It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<Expression<Func<AuditLog, bool>>>(),
                It.IsAny<Func<IQueryable<AuditLog>, IOrderedQueryable<AuditLog>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GET_AUDIT_LOGS_FAILED");
    }

    [Fact]
    public async Task Handle_ShouldMapEntities_ToDtosCorrectly()
    {
        var userId = Guid.NewGuid();
        var log = new AuditLog(
            action: "CustomerCreated",
            entityType: "Customer",
            success: true,
            userId: userId,
            username: "admin",
            entityId: "123",
            ipAddress: "192.168.1.1",
            correlationId: "corr-xyz");

        SetupPagedReturn(new List<AuditLog> { log }, 1);

        var result = await _handler.Handle(new GetAuditLogsQuery(), CancellationToken.None);

        var dto = result.Value.Items.First();
        dto.Action.Should().Be("CustomerCreated");
        dto.EntityType.Should().Be("Customer");
        dto.EntityId.Should().Be("123");
        dto.Username.Should().Be("admin");
        dto.UserId.Should().Be(userId);
        dto.IpAddress.Should().Be("192.168.1.1");
        dto.CorrelationId.Should().Be("corr-xyz");
        dto.Success.Should().BeTrue();
    }
}
