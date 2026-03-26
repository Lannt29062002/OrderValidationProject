using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R1: CustomerId, WarehouseCode không được null/empty. OrderDate không được là ngày tương lai.
/// </summary>
public class R1_BasicInfoRule : IOrderValidationRule
{
    public string RuleCode => "R1";

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (string.IsNullOrWhiteSpace(order.CustomerId))
        {
            yield return new ValidationError(RuleCode, "CustomerId không được để trống", "CustomerId");
        }

        if (string.IsNullOrWhiteSpace(order.WarehouseCode))
        {
            yield return new ValidationError(RuleCode, "WarehouseCode không được để trống", "WarehouseCode");
        }

        if (order.OrderDate.Date > DateTime.Today)
        {
            yield return new ValidationError(RuleCode, "OrderDate không được là ngày tương lai", "OrderDate");
        }
    }
}
