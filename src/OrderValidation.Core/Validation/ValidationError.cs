namespace OrderValidation.Core.Validation;

public class ValidationError
{
    public string Code { get; set; } = string.Empty;      // "R1", "R2", ...
    public string Message { get; set; } = string.Empty;   // mô tả lỗi
    public string? Field { get; set; }     // tên field/SKU liên quan (nullable)

    public ValidationError() { }

    public ValidationError(string code, string message, string? field = null)
    {
        Code = code;
        Message = message;
        Field = field;
    }
}
