using System.Linq.Expressions;
using Application.Customers;
using Domain.Entities;
using Domain.Repositories;
using Domain.ValueObjects;
using FluentAssertions;
using Moq;

namespace Core.Tests.Application.Customers;

public class GetAllCustomersQueryHandlerTests
{
    private readonly Mock<IRepository<Customer>> _repo = new();
    private readonly GetAllCustomersQueryHandler _handler;

    public GetAllCustomersQueryHandlerTests()
    {
        _handler = new GetAllCustomersQueryHandler(_repo.Object);
    }

    private static Customer CreateCustomer(string lastName, bool active = true)
    {
        var c = new Customer(
            "John",
            lastName,
            Email.Create($"{Guid.NewGuid():N}@example.com"),
            new Address("Street", "City", "ST", "12345", "USA"));

        if (!active) c.Deactivate();
        return c;
    }

    [Fact]
    public async Task Handle_ShouldReturnPagedResult_WithMappedDtos()
    {
        var customers = new List<Customer>
        {
            CreateCustomer("Doe"),
            CreateCustomer("Smith")
        };

        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(customers);

        _repo.Setup(r => r.CountAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        var result = await _handler.Handle(new GetAllCustomersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
        result.Value.PageNumber.Should().Be(1);
        result.Value.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Handle_ShouldReturnEmptyResult_WhenNoCustomers()
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Customer>());

        _repo.Setup(r => r.CountAsync(
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(new GetAllCustomersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_ShouldReturnFailure_WhenRepositoryThrows()
    {
        _repo.Setup(r => r.GetPagedAsync(
                It.IsAny<int>(), It.IsAny<int>(),
                It.IsAny<Expression<Func<Customer, bool>>>(),
                It.IsAny<Func<IQueryable<Customer>, IOrderedQueryable<Customer>>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("DB failure"));

        var result = await _handler.Handle(new GetAllCustomersQuery(), CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be("GET_ALL_FAILED");
    }
}
