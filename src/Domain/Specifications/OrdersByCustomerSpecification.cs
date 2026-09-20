using Domain.Entities;

namespace Domain.Specifications;

public sealed class OrdersByCustomerSpecification : BaseSpecification<Order>
{
    public OrdersByCustomerSpecification(Guid customerId)
        : base(o => o.CustomerId == customerId)
    {
        AddInclude(o => o.Lines);
        ApplyOrderByDescending(o => o.OrderDate);
    }
}
