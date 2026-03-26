using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R2: RequiredDate phải sau OrderDate tối thiểu theo kênh:
/// - Online ≥ 1 ngày
/// - Offline ≥ 2 ngày  
/// - EDI ≥ 3 ngày làm việc (bỏ qua Thứ 7, Chủ nhật)
/// </summary>
public class R2_RequiredDateRule : IOrderValidationRule
{
    public string RuleCode => "R2";

    public IEnumerable<ValidationError> Validate(Order order)
    {
        int requiredDays = order.Channel switch
        {
            SalesChannel.Online => 1,
            SalesChannel.Offline => 2,
            SalesChannel.EDI => 3,
            _ => 1
        };

        bool isValid;
        string channelName = order.Channel.ToString();

        if (order.Channel == SalesChannel.EDI)
        {
            // EDI tính theo ngày làm việc
            int businessDays = CountBusinessDays(order.OrderDate, order.RequiredDate);
            isValid = businessDays >= requiredDays;

            if (!isValid)
            {
                yield return new ValidationError(
                    RuleCode,
                    $"EDI yêu cầu tối thiểu {requiredDays} ngày làm việc (hiện tại: {businessDays} ngày)",
                    "RequiredDate"
                );
            }
        }
        else
        {
            // Online/Offline tính theo ngày dương lịch
            int calendarDays = (order.RequiredDate.Date - order.OrderDate.Date).Days;
            isValid = calendarDays >= requiredDays;

            if (!isValid)
            {
                yield return new ValidationError(
                    RuleCode,
                    $"{channelName} yêu cầu RequiredDate sau OrderDate tối thiểu {requiredDays} ngày (hiện tại: {calendarDays} ngày)",
                    "RequiredDate"
                );
            }
        }
    }

    /// <summary>
    /// Đếm số ngày làm việc giữa 2 ngày (không tính T7, CN)
    /// </summary>
    private int CountBusinessDays(DateTime from, DateTime to)
    {
        if (to.Date <= from.Date)
            return 0;

        int businessDays = 0;
        DateTime current = from.Date.AddDays(1); // Bắt đầu từ ngày sau OrderDate

        while (current <= to.Date)
        {
            if (current.DayOfWeek != DayOfWeek.Saturday && 
                current.DayOfWeek != DayOfWeek.Sunday)
            {
                businessDays++;
            }
            current = current.AddDays(1);
        }

        return businessDays;
    }
}
