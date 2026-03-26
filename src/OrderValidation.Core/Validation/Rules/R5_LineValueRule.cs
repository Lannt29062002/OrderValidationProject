using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R5: Mỗi dòng: Quantity > 0, UnitPrice > 0, DiscountPct trong khoảng [0, 100],
/// Unit phải là một trong "EA", "CASE", "PALLET".
/// </summary>
public class R5_LineValueRule : IOrderValidationRule
{
    public string RuleCode => "R5";
    private static readonly HashSet<string> ValidUnits = new(StringComparer.OrdinalIgnoreCase) 
    { 
        "EA", "CASE", "PALLET" 
    };

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (order.Lines == null || order.Lines.Count == 0)
            yield break;

        for (int i = 0; i < order.Lines.Count; i++)
        {
            var line = order.Lines[i];
            if (line == null)
            {
                yield return new ValidationError(RuleCode, $"Lines[{i}] không được null", $"Lines[{i}]");
                continue;
            }
            string skuRef = !string.IsNullOrWhiteSpace(line.Sku) ? line.Sku : $"Lines[{i}]";

            if (line.Quantity <= 0)
            {
                yield return new ValidationError(
                    RuleCode,
                    $"{skuRef}: Quantity phải lớn hơn 0 (hiện tại: {line.Quantity})",
                    skuRef
                );
            }

            if (line.UnitPrice <= 0)
            {
                yield return new ValidationError(
                    RuleCode,
                    $"{skuRef}: UnitPrice phải lớn hơn 0 (hiện tại: {line.UnitPrice})",
                    skuRef
                );
            }

            if (line.DiscountPct < 0 || line.DiscountPct > 100)
            {
                yield return new ValidationError(
                    RuleCode,
                    $"{skuRef}: DiscountPct phải trong khoảng [0, 100] (hiện tại: {line.DiscountPct})",
                    skuRef
                );
            }

            if (string.IsNullOrWhiteSpace(line.Unit) || !ValidUnits.Contains(line.Unit))
            {
                yield return new ValidationError(
                    RuleCode,
                    $"{skuRef}: Unit phải là một trong 'EA', 'CASE', 'PALLET' (hiện tại: '{line.Unit}')",
                    skuRef
                );
            }
        }
    }
}
