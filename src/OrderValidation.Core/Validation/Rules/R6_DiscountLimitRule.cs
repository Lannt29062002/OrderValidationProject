using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R6: Chiết khấu tối đa theo loại khách: Retail ≤ 10%, Wholesale ≤ 30%, Internal không giới hạn.
/// Lỗi phải kèm tên SKU vi phạm.
/// </summary>
public class R6_DiscountLimitRule : IOrderValidationRule
{
    public string RuleCode => "R6";

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (order.Lines == null || order.Lines.Count == 0)
            yield break;

        // Internal không giới hạn chiết khấu
        if (order.CustomerType == CustomerType.Internal)
            yield break;

        decimal maxDiscount = order.CustomerType switch
        {
            CustomerType.Retail => 10m,
            CustomerType.Wholesale => 30m,
            _ => 100m // Không giới hạn cho các loại khác
        };

        foreach (var line in order.Lines)
        {
            if (line == null) continue;
            if (line.DiscountPct > maxDiscount)
            {
                string sku = !string.IsNullOrWhiteSpace(line.Sku) ? line.Sku : "Unknown";
                yield return new ValidationError(
                    RuleCode,
                    $"{sku} vượt chiết khấu tối đa {maxDiscount}% ({order.CustomerType}) - hiện tại: {line.DiscountPct}%",
                    sku
                );
            }
        }
    }
}
