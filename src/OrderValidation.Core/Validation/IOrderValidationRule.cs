using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation;

/// <summary>
/// Interface cho mỗi rule validation - theo Open/Closed Principle
/// </summary>
public interface IOrderValidationRule
{
    /// <summary>
    /// Mã rule (R1, R2, ...)
    /// </summary>
    string RuleCode { get; }

    /// <summary>
    /// Validate order và trả về danh sách lỗi (nếu có)
    /// </summary>
    IEnumerable<ValidationError> Validate(Order order);
}
