using OrderValidation.Core.Models;

namespace OrderValidation.Core.Validation.Rules;

/// <summary>
/// R8: Kênh EDI bắt buộc Metadata có key "edi_sender_id" và "edi_version".
/// Giá trị edi_version chỉ được là "2.0" hoặc "3.0".
/// </summary>
public class R8_EdiMetadataRule : IOrderValidationRule
{
    public string RuleCode => "R8";
    
    private const string EdiSenderIdKey = "edi_sender_id";
    private const string EdiVersionKey = "edi_version";
    private static readonly HashSet<string> ValidVersions = new() { "2.0", "3.0" };

    public IEnumerable<ValidationError> Validate(Order order)
    {
        // Chỉ áp dụng cho kênh EDI
        if (order.Channel != SalesChannel.EDI)
            yield break;

        // Kiểm tra Metadata null
        if (order.Metadata == null)
        {
            yield return new ValidationError(
                RuleCode,
                $"Thiếu metadata bắt buộc: {EdiSenderIdKey}",
                EdiSenderIdKey
            );
            yield return new ValidationError(
                RuleCode,
                $"Thiếu metadata bắt buộc: {EdiVersionKey}",
                EdiVersionKey
            );
            yield break;
        }

        // Kiểm tra edi_sender_id
        if (!order.Metadata.TryGetValue(EdiSenderIdKey, out var senderId) || 
            string.IsNullOrWhiteSpace(senderId))
        {
            yield return new ValidationError(
                RuleCode,
                $"Thiếu metadata bắt buộc: {EdiSenderIdKey}",
                EdiSenderIdKey
            );
        }

        // Kiểm tra edi_version
        if (!order.Metadata.TryGetValue(EdiVersionKey, out var version) || 
            string.IsNullOrWhiteSpace(version))
        {
            yield return new ValidationError(
                RuleCode,
                $"Thiếu metadata bắt buộc: {EdiVersionKey}",
                EdiVersionKey
            );
        }
        else if (!ValidVersions.Contains(version))
        {
            yield return new ValidationError(
                RuleCode,
                $"Giá trị {EdiVersionKey} không hợp lệ: '{version}'. Chỉ chấp nhận '2.0' hoặc '3.0'",
                EdiVersionKey
            );
        }
    }
}
