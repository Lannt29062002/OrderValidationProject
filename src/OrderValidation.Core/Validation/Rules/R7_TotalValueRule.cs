using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R7: Tổng giá trị đơn = Σ(Quantity × UnitPrice × (1 - DiscountPct/100))
/// Retail ≤ 100.000.000 đ, Wholesale ≤ 2.000.000.000 đ, Internal không giới hạn.
/// </summary>
public class R7_TotalValueRule : IOrderValidationRule
{
    public string RuleCode => "R7";
    
    private const decimal RetailMaxValue = 100_000_000m;      // 100 triệu
    private const decimal WholesaleMaxValue = 2_000_000_000m; // 2 tỷ

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (order.Lines == null || order.Lines.Count == 0)
            yield break;

        // Internal không giới hạn
        if (order.CustomerType == CustomerType.Internal)
            yield break;

        decimal totalValue = CalculateTotalValue(order.Lines);

        decimal maxValue = order.CustomerType switch
        {
            CustomerType.Retail => RetailMaxValue,
            CustomerType.Wholesale => WholesaleMaxValue,
            _ => decimal.MaxValue
        };

        if (totalValue > maxValue)
        {
            yield return new ValidationError(
                RuleCode,
                $"Tổng giá trị đơn hàng ({totalValue:N0} đ) vượt quá giới hạn cho {order.CustomerType} ({maxValue:N0} đ)",
                "TotalValue"
            );
        }
    }

    private decimal CalculateTotalValue(List<OrderLine> lines)
    {
        decimal total = 0;
        foreach (var line in lines)
        {
            if (line == null) continue;
            decimal lineValue = line.Quantity * line.UnitPrice * (1 - line.DiscountPct / 100m);
            total += lineValue;
        }
        return total;
    }
}
