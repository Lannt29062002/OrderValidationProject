namespace OrderValidation.Core.Validation;

public class ValidationResult
{
    public bool IsValid => !Errors.Any();
    public List<ValidationError> Errors { get; } = new();

    public void AddError(string code, string message, string? field = null)
        => Errors.Add(new ValidationError(code, message, field));

    public void AddErrors(IEnumerable<ValidationError> errors)
    {
        foreach (var error in errors)
        {
            Errors.Add(error);
        }
    }
}
