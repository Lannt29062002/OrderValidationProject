using OrderValidation.Core.Models;

namespace OrderValidation.Demo;

/// <summary>
/// Chứa tất cả test cases cho việc demo validation
/// </summary>
public static class TestCases
{
    /// <summary>
    /// Lấy tất cả test cases theo từng rule
    /// </summary>
    public static IEnumerable<(string Name, string Description, Order Order)> GetAllTestCases()
    {
        // R1: Basic Info
        foreach (var tc in GetR1TestCases()) yield return tc;
        
        // R2: Required Date
        foreach (var tc in GetR2TestCases()) yield return tc;
        
        // R3: Sales Rep
        foreach (var tc in GetR3TestCases()) yield return tc;
        
        // R4: Lines
        foreach (var tc in GetR4TestCases()) yield return tc;
        
        // R5: Line Values
        foreach (var tc in GetR5TestCases()) yield return tc;
        
        // R6: Discount Limit
        foreach (var tc in GetR6TestCases()) yield return tc;
        
        // R7: Total Value
        foreach (var tc in GetR7TestCases()) yield return tc;
        
        // R8: EDI Metadata
        foreach (var tc in GetR8TestCases()) yield return tc;
        
        // R9: PALLET Quantity
        foreach (var tc in GetR9TestCases()) yield return tc;
        
        // R10: Collect All Errors
        foreach (var tc in GetR10TestCases()) yield return tc;
    }

    #region R1 - Basic Info

    public static IEnumerable<(string Name, string Description, Order Order)> GetR1TestCases()
    {
        yield return ("R1.1", "CustomerId trống", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R1.2", "WarehouseCode null", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = null,
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R1.3", "OrderDate là ngày tương lai", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today.AddDays(1),
            RequiredDate = DateTime.Today.AddDays(3),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R2 - Required Date

    public static IEnumerable<(string Name, string Description, Order Order)> GetR2TestCases()
    {
        yield return ("R2.1", "Online - RequiredDate = OrderDate (cần ≥1 ngày)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today,
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R2.2", "Offline - chỉ 1 ngày (cần ≥2 ngày)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R2.3", "EDI Thứ 6 -> Thứ 2 = 1 ngày làm việc (cần ≥3)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Wholesale,
            Channel = SalesChannel.EDI,
            WarehouseCode = "WH01",
            OrderDate = new DateTime(2025, 3, 28),
            RequiredDate = new DateTime(2025, 3, 31),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string> { { "edi_sender_id", "X1" }, { "edi_version", "2.0" } }
        });
    }

    #endregion

    #region R3 - Sales Rep

    public static IEnumerable<(string Name, string Description, Order Order)> GetR3TestCases()
    {
        yield return ("R3.1", "Offline không có SalesRepId", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = null,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(2),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R3.2", "Online có SalesRepId (phải null)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R4 - Lines

    public static IEnumerable<(string Name, string Description, Order Order)> GetR4TestCases()
    {
        yield return ("R4.1", "Không có dòng sản phẩm", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>(),
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R4.2", "SKU trùng nhau", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 0, Unit = "EA" },
                new() { Sku = "A01", Quantity = 2, UnitPrice = 200_000m, DiscountPct = 0, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R4.3", "SKU trống", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 0, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R5 - Line Values

    public static IEnumerable<(string Name, string Description, Order Order)> GetR5TestCases()
    {
        yield return ("R5.1", "Quantity = 0", CreateOrderWithLine(
            new OrderLine { Sku = "A01", Quantity = 0, UnitPrice = 100_000m, DiscountPct = 0, Unit = "EA" }));

        yield return ("R5.2", "UnitPrice âm", CreateOrderWithLine(
            new OrderLine { Sku = "A01", Quantity = 1, UnitPrice = -100, DiscountPct = 0, Unit = "EA" }));

        yield return ("R5.3", "DiscountPct = 150%", CreateOrderWithLine(
            new OrderLine { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 150, Unit = "EA" },
            CustomerType.Internal));

        yield return ("R5.4", "Unit không hợp lệ (BOX)", CreateOrderWithLine(
            new OrderLine { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 0, Unit = "BOX" }));
    }

    #endregion

    #region R6 - Discount Limit

    public static IEnumerable<(string Name, string Description, Order Order)> GetR6TestCases()
    {
        yield return ("R6.1", "Retail chiết khấu 15% (max 10%)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(2),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 15, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R6.2", "Wholesale chiết khấu 35% (max 30%)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Wholesale,
            Channel = SalesChannel.Offline,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(2),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 35, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R6.3", "Internal chiết khấu 90% (không giới hạn) - VALID", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Internal,
            Channel = SalesChannel.Offline,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(2),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 90, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R7 - Total Value

    public static IEnumerable<(string Name, string Description, Order Order)> GetR7TestCases()
    {
        yield return ("R7.1", "Retail tổng 150M (max 100M)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 150, UnitPrice = 1_000_000m, DiscountPct = 0, Unit = "EA" }
            },
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R8 - EDI Metadata

    public static IEnumerable<(string Name, string Description, Order Order)> GetR8TestCases()
    {
        yield return ("R8.1", "EDI thiếu edi_sender_id", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Wholesale,
            Channel = SalesChannel.EDI,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(5),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string> { { "edi_version", "2.0" } }
        });

        yield return ("R8.2", "EDI version không hợp lệ (1.0)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Wholesale,
            Channel = SalesChannel.EDI,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(5),
            Lines = CreateDefaultLines(),
            Metadata = new Dictionary<string, string> { { "edi_sender_id", "X1" }, { "edi_version", "1.0" } }
        });
    }

    #endregion

    #region R9 - PALLET Quantity

    public static IEnumerable<(string Name, string Description, Order Order)> GetR9TestCases()
    {
        yield return ("R9.1", "PALLET qty=6 (không phải bội số 4)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Internal,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 6, UnitPrice = 100_000m, DiscountPct = 0, Unit = "PALLET" }
            },
            Metadata = new Dictionary<string, string>()
        });

        yield return ("R9.2", "PALLET qty=8 (hợp lệ) - VALID", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Internal,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "A01", Quantity = 8, UnitPrice = 100_000m, DiscountPct = 0, Unit = "PALLET" }
            },
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region R10 - Collect All Errors

    public static IEnumerable<(string Name, string Description, Order Order)> GetR10TestCases()
    {
        yield return ("R10", "Đơn hàng vi phạm nhiều rules cùng lúc (R1, R2, R3, R5, R6, R9)", new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = null,
            WarehouseCode = "",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new() { Sku = "X01", Quantity = 0, UnitPrice = -100, DiscountPct = 20, Unit = "PALLET" }
            },
            Metadata = new Dictionary<string, string>()
        });
    }

    #endregion

    #region Helpers

    private static List<OrderLine> CreateDefaultLines()
    {
        return new List<OrderLine>
        {
            new() { Sku = "A01", Quantity = 1, UnitPrice = 100_000m, DiscountPct = 0, Unit = "EA" }
        };
    }

    private static Order CreateOrderWithLine(OrderLine line, CustomerType customerType = CustomerType.Retail)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = customerType,
            Channel = SalesChannel.Online,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine> { line },
            Metadata = new Dictionary<string, string>()
        };
    }

    #endregion
}
