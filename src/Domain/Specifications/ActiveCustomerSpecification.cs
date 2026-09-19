using Domain.Entities;

namespace Domain.Specifications;

public sealed class ActiveCustomerSpecification : BaseSpecification<Customer>
{
    public ActiveCustomerSpecification()
        : base(c => c.IsActive)
    {
        ApplyOrderBy(c => c.LastName);
    }
}
