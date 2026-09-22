using Domain.ValueObjects;

namespace Application.Common;

public interface IDiscountService
{
    Task<Discount?> ResolveCodeAsync(
        string code,
        CancellationToken cancellationToken = default);
}
