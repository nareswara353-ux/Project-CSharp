using Domain.Entities;
using Domain.Specifications;
using Domain.ValueObjects;
using FluentAssertions;

namespace Core.Tests.Domain.Specifications;

public class ActiveCustomerSpecificationTests
{
    [Fact]
    public void Specification_ShouldHaveCriteria()
    {
        var spec = new ActiveCustomerSpecification();

        spec.Criteria.Should().NotBeNull();
    }

    [Fact]
    public void Specification_ShouldMatchActiveCustomer()
    {
        var spec = new ActiveCustomerSpecification();
        var customer = new Customer(
            "John",
            "Doe",
            Email.Create("john@example.com"),
            new Address("123 Main St", "New York", "NY", "10001", "USA"));

        var compiled = spec.Criteria!.Compile();

        compiled(customer).Should().BeTrue();
    }

    [Fact]
    public void Specification_ShouldNotMatchInactiveCustomer()
    {
        var spec = new ActiveCustomerSpecification();
        var customer = new Customer(
            "John",
            "Doe",
            Email.Create("john@example.com"),
            new Address("123 Main St", "New York", "NY", "10001", "USA"));
        customer.Deactivate();

        var compiled = spec.Criteria!.Compile();

        compiled(customer).Should().BeFalse();
    }

    [Fact]
    public void Specification_ShouldHaveOrderBySet()
    {
        var spec = new ActiveCustomerSpecification();

        spec.OrderBy.Should().NotBeNull();
    }
}
