using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R3: Kênh Offline bắt buộc có SalesRepId. Kênh Online SalesRepId phải là null.
/// </summary>
public class R3_SalesRepRule : IOrderValidationRule
{
    public string RuleCode => "R3";

    public IEnumerable<ValidationError> Validate(Order order)
    {
        if (order.Channel == SalesChannel.Offline)
        {
            if (string.IsNullOrWhiteSpace(order.SalesRepId))
            {
                yield return new ValidationError(
                    RuleCode,
                    "Kênh Offline bắt buộc phải có SalesRepId",
                    "SalesRepId"
                );
            }
        }
        else if (order.Channel == SalesChannel.Online)
        {
            if (!string.IsNullOrWhiteSpace(order.SalesRepId))
            {
                yield return new ValidationError(
                    RuleCode,
                    "Kênh Online không được có SalesRepId",
                    "SalesRepId"
                );
            }
        }
        // EDI không có yêu cầu về SalesRepId
    }
}
