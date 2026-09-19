using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Specifications;

public sealed class CustomerByEmailSpecification : BaseSpecification<Customer>
{
    public CustomerByEmailSpecification(Email email)
        : base(c => c.Email.Value == email.Value)
    {
    }

    public CustomerByEmailSpecification(string email)
        : base(c => c.Email.Value == email.ToLowerInvariant())
    {
    }
}
