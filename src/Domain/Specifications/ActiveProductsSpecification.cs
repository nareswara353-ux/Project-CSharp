using Domain.Entities;

namespace Domain.Specifications;

public sealed class ActiveProductsSpecification : BaseSpecification<Product>
{
    public ActiveProductsSpecification()
        : base(p => p.IsActive)
    {
        ApplyOrderBy(p => p.Name);
    }
}
