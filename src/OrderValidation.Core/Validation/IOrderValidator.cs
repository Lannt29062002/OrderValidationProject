using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation;

public interface IOrderValidator
{
    ValidationResult Validate(Order order);
}
