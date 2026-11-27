using Application.Products;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Products;

public class UpdateProductCommandHandlerTests
{
    private readonly Mock<IRepository<Product>> _repo = new();
    private readonly UpdateProductCommandHandler _handler;

    public UpdateProductCommandHandlerTests()
    {
        _handler = new UpdateProductCommandHandler(_repo.Object);
    }

    private static Product CreateProduct()
    {
        return new Product(
            "Widget",
            "Description",
            ProductSku.Create("PRD-001"),
            new Money(10m, "USD"),
            100);
    }

    [Fact]
    public async Task Handle_ShouldUpdateProduct_WhenExists()
    {
        var product = CreateProduct();
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated Widget",
            Description = "Updated",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 50
        };

        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Name.Should().Be("Updated Widget");
        product.Price.Amount.Should().Be(99.99m);
        product.StockQuantity.Should().Be(50);
        _repo.Verify(r => r.Update(product), Times.Once);
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenProductNotFound()
    {
        var command = new UpdateProductCommand
        {
            Id = Guid.NewGuid(),
            Name = "Updated Widget",
            Description = "Updated",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 50
        };

        _repo.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("NOT_FOUND");
        _repo.Verify(r => r.Update(It.IsAny<Product>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnValidationFailure_WhenDataInvalid()
    {
        var product = CreateProduct();
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "", // invalid
            Description = "Updated",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 50
        };

        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("VALIDATION_ERROR");
        _repo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        var product = CreateProduct();
        var command = new UpdateProductCommand
        {
            Id = product.Id,
            Name = "Updated Widget",
            Description = "Updated",
            Price = 99.99m,
            Currency = "USD",
            StockQuantity = 50
        };

        _repo.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);
        _repo.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("UPDATE_FAILED");
    }
}
