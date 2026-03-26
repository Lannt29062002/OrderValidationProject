using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R4: Danh sách Lines tối thiểu 1 dòng, tối đa 50 dòng. 
/// Không có Sku trùng nhau. Sku không được null/empty.
/// </summary>
public class R4_LinesRule : IOrderValidationRule
{
    public string RuleCode => "R4";
    private const int MinLines = 1;
    private const int MaxLines = 50;

    public IEnumerable<ValidationError> Validate(Order order)
    {
        // Kiểm tra Lines null hoặc rỗng
        if (order.Lines == null || order.Lines.Count == 0)
        {
            yield return new ValidationError(
                RuleCode,
                $"Đơn hàng phải có tối thiểu {MinLines} dòng sản phẩm",
                "Lines"
            );
            yield break; // Không cần kiểm tra tiếp nếu không có Lines
        }

        // Kiểm tra số lượng dòng
        if (order.Lines.Count > MaxLines)
        {
            yield return new ValidationError(
                RuleCode,
                $"Đơn hàng không được vượt quá {MaxLines} dòng sản phẩm (hiện tại: {order.Lines.Count})",
                "Lines"
            );
        }

        // Kiểm tra Sku null/empty
        var skus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        for (int i = 0; i < order.Lines.Count; i++)
        {
            var line = order.Lines[i];
            if (line == null)
            {
                yield return new ValidationError(RuleCode, $"Lines[{i}] không được null", $"Lines[{i}]");
                continue;
            }

            if (string.IsNullOrWhiteSpace(line.Sku))
            {
                yield return new ValidationError(
                    RuleCode,
                    $"Dòng {i + 1}: Sku không được để trống",
                    $"Lines[{i}].Sku"
                );
                continue;
            }

            // Kiểm tra Sku trùng
            if (!skus.Add(line.Sku))
            {
                yield return new ValidationError(
                    RuleCode,
                    $"Sku '{line.Sku}' bị trùng trong đơn hàng",
                    line.Sku
                );
            }
        }
    }
}
