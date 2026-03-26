using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R9: Dòng có Unit = "PALLET" thì Quantity phải là bội số của 4.
/// Lỗi phải kèm tên SKU và giá trị Quantity thực tế.
/// </summary>
public class R9_PalletQuantityRule : IOrderValidationRule
{
    public string RuleCode => "R9";
    private const int PalletMultiple = 4;

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (order.Lines == null || order.Lines.Count == 0)
            yield break;

        foreach (var line in order.Lines)
        {
            if (line == null) continue;
            if (string.Equals(line.Unit, "PALLET", StringComparison.OrdinalIgnoreCase))
            {
                if (line.Quantity % PalletMultiple != 0)
                {
                    string sku = !string.IsNullOrWhiteSpace(line.Sku) ? line.Sku : "Unknown";
                    yield return new ValidationError(
                        RuleCode,
                        $"{sku} qty={line.Quantity} không phải bội số của {PalletMultiple}",
                        sku
                    );
                }
            }
        }
    }
}
