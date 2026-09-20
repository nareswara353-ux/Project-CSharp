using Domain.Entities;
using Domain.ValueObjects;

namespace Domain.Specifications;

public sealed class ProductBySkuSpecification : BaseSpecification<Product>
{
    public ProductBySkuSpecification(ProductSku sku)
        : base(p => p.Sku.Value == sku.Value)
    {
    }

    public ProductBySkuSpecification(string sku)
        : base(p => p.Sku.Value == sku.Trim().ToUpperInvariant())
    {
    }
}
