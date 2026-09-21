using Application.Products;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Products;

public class GetAllProductsQueryHandlerTests
{
    private readonly Mock<IRepository<Product>> _repo = new();
    private readonly GetAllProductsQueryHandler _handler;

    public GetAllProductsQueryHandlerTests()
    {
        _handler = new GetAllProductsQueryHandler(_repo.Object);
    }

    private static Product CreateProduct(string sku, bool active = true)
    {
        var product = new Product(
            "Widget",
            "Description",
            ProductSku.Create(sku),
            new Money(10m, "USD"),
            100);

        if (!active)
            product.Deactivate();

        return product;
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WhenProductsExist()
    {
        var products = new List<Product>
        {
            CreateProduct("PRD-001"),
            CreateProduct("PRD-002"),
            CreateProduct("PRD-003")
        };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(products);

        _repo.Setup(r => r.CountAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var result = await _handler.Handle(
            new GetAllProductsQuery { PageNumber = 1, PageSize = 10 },
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(3);
        result.Value.TotalCount.Should().Be(3);
        result.Value.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoProducts()
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product>());

        _repo.Setup(r => r.CountAsync(
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(
            new GetAllProductsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<System.Linq.Expressions.Expression<Func<Product, bool>>>(),
                It.IsAny<Func<IQueryable<Product>, IOrderedQueryable<Product>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(
            new GetAllProductsQuery(),
            CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GET_ALL_FAILED");
    }
}
