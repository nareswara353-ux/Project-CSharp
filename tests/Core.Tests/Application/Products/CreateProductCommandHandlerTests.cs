using Application.Products;
using Domain.Entities;
using Domain.Repositories;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Products;

public class CreateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repo = new();
    private readonly CreateProductCommandHandler _handler;

    public CreateProductCommandHandlerTests()
    {
        _handler = new CreateProductCommandHandler(_repo.Object);
    }

    [Fact]
    public async Task Handle_ShouldReturnSuccess_WhenProductCreated()
    {
        var command = new CreateProductCommand
        {
            Name = "Widget",
            Description = "A fine widget",
            Sku = "PRD-001",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 10
        };

        _repo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.FromResult(1));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();
        _repo.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenSkuInvalid()
    {
        var command = new CreateProductCommand
        {
            Name = "Widget",
            Sku = "invalid sku with spaces",
            Price = 10m,
            Currency = "USD"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenPriceNegative()
    {
        var command = new CreateProductCommand
        {
            Name = "Widget",
            Sku = "PRD-001",
            Price = -10m,
            Currency = "USD"
        };

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        var command = new CreateProductCommand
        {
            Name = "Widget",
            Sku = "PRD-001",
            Price = 10m,
            Currency = "USD"
        };

        _repo.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("CREATE_FAILED");
    }
}
