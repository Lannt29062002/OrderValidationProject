using OrderValidation.Core.Models;
using OrderValidation.Core.Validation.Rules;

namespace OrderValidation.Core.Validation;

/// <summary>
/// Order Validator - implements IOrderValidator
/// Sử dụng pattern Open/Closed: mỗi rule là một class riêng biệt
/// Để thêm rule mới, chỉ cần tạo class implement IOrderValidationRule và thêm vào danh sách rules
/// </summary>
public class OrderValidator : IOrderValidator
{
    private readonly List<IOrderValidationRule> _rules;

    public OrderValidator()
    {
        // Khởi tạo tất cả các rules theo thứ tự R1 -> R9
        _rules = new List<IOrderValidationRule>
        {
            new R1_BasicInfoRule(),
            new R2_RequiredDateRule(),
            new R3_SalesRepRule(),
            new R4_LinesRule(),
            new R5_LineValueRule(),
            new R6_DiscountLimitRule(),
            new R7_TotalValueRule(),
            new R8_EdiMetadataRule(),
            new R9_PalletQuantityRule()
        };
    }

    /// <summary>
    /// Constructor cho phép inject custom rules (dùng cho testing hoặc mở rộng)
    /// </summary>
    public OrderValidator(IEnumerable<IOrderValidationRule> rules)
    {
        _rules = rules.ToList();
    }

    public ValidationResult Validate(Order order)
    {
        var result = new ValidationResult();

        if (order == null)
        {
            result.AddError("R0", "Order không được null", "Order");
            return result;
        }

        // R10: Collect ALL errors - không dừng ở lỗi đầu tiên
        foreach (var rule in _rules)
        {
            var errors = rule.Validate(order);
            result.AddErrors(errors);
        }

        return result;
    }
}
