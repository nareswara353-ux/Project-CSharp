using Domain.ValueObjects;

namespace Application.Pricing;

public class PriceCalculator
{
    public Money CalculateTotal(Money subtotal, Discount? discount = null)
    {
        if (subtotal is null)
            throw new ArgumentNullException(nameof(subtotal));

        if (discount is null)
            return subtotal;

        return discount.ApplyTo(subtotal);
    }

    public Money CalculateTotalWithTax(
        Money subtotal,
        Discount? discount,
        Percentage taxRate)
    {
        if (subtotal is null)
            throw new ArgumentNullException(nameof(subtotal));

        if (taxRate is null)
            throw new ArgumentNullException(nameof(taxRate));

        var afterDiscount = CalculateTotal(subtotal, discount);
        return taxRate.AddTo(afterDiscount);
    }
}
