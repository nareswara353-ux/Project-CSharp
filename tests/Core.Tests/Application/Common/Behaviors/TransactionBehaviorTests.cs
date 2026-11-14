using Application.Common.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Core.Tests.Application.Common.Behaviors;

public class TransactionBehaviorTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ILogger<TransactionBehavior<TransactionalCommand, string>>> _loggerMock = new();

    public record TransactionalCommand : IRequest<string>, ITransactionalRequest;
    public record NonTransactionalCommand : IRequest<string>;

    [Fact]
    public async Task Handle_ShouldNotBeginTransaction_WhenRequestIsNotTransactional()
    {
        var behavior = new TransactionBehavior<NonTransactionalCommand, string>(_uowMock.Object, _loggerMock.Object);

        var response = await behavior.Handle(
            new NonTransactionalCommand(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldBeginAndCommitTransaction_WhenRequestIsTransactional()
    {
        var behavior = new TransactionBehavior<TransactionalCommand, string>(_uowMock.Object, _loggerMock.Object);

        _uowMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var response = await behavior.Handle(
            new TransactionalCommand(),
            _ => Task.FromResult("ok"),
            CancellationToken.None);

        response.Should().Be("ok");
        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldRollbackTransaction_WhenNextThrows()
    {
        var behavior = new TransactionBehavior<TransactionalCommand, string>(_uowMock.Object, _loggerMock.Object);

        _uowMock.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        _uowMock.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        Func<Task> act = async () => await behavior.Handle(
            new TransactionalCommand(),
            _ => throw new InvalidOperationException("test error"),
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("test error");

        _uowMock.Verify(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _uowMock.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Never);
        _uowMock.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
