namespace OrderValidation.Core.Models;

public class OrderLine
{
    public string? Sku { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPct { get; set; } // 0-100
    public string? Unit { get; set; } // "EA" | "CASE" | "PALLET"
}
