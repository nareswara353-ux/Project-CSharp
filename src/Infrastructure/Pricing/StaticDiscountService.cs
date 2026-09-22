using Application.Common;
using Domain.ValueObjects;

namespace Infrastructure.Pricing;

public class StaticDiscountService : IDiscountService
{
    private static readonly Dictionary<string, Discount> Discounts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["WELCOME10"] = Discount.Percentage(10m, "WELCOME10"),
            ["SAVE5"] = Discount.FixedAmount(5m, "USD", "SAVE5"),
            ["VIP20"] = Discount.Percentage(20m, "VIP20")
        };

    public Task<Discount?> ResolveCodeAsync(
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Task.FromResult<Discount?>(null);

        var normalized = code.Trim().ToUpperInvariant();
        Discounts.TryGetValue(normalized, out var discount);

        return Task.FromResult(discount);
    }
}
