namespace OrderValidation.Core.Models;

public class Order
{
    public Guid Id { get; set; }
    public string? CustomerId { get; set; }
    public CustomerType CustomerType { get; set; }
    public SalesChannel Channel { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime RequiredDate { get; set; }
    public string? WarehouseCode { get; set; }
    public string? SalesRepId { get; set; } // nullable - Online không cần
    public List<OrderLine>? Lines { get; set; }
    public Dictionary<string, string>? Metadata { get; set; }
}
