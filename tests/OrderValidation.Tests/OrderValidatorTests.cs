using OrderValidation.Core.Models;
using OrderValidation.Core.Validation;

namespace OrderValidation.Tests;

public class OrderValidatorTests
{
    private readonly IOrderValidator _validator;

    public OrderValidatorTests()
    {
        _validator = new OrderValidator();
    }

    #region Test Case 1 — Retail, Online, hợp lệ

    [Fact]
    public void Case1_Retail_Online_Valid_ShouldPass()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            SalesRepId = null,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "A01",
                    Quantity = 2,
                    UnitPrice = 500_000m,
                    DiscountPct = 10,
                    Unit = "EA"
                }
            },
            Metadata = new Dictionary<string, string>()
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    #endregion

    #region Test Case 2 — Retail vượt chiết khấu + PALLET sai số lượng

    [Fact]
    public void Case2_Retail_DiscountExceeded_And_PalletQtyInvalid_ShouldFail()
    {
        // Arrange
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST002",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = "SR01",
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(2),
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "B01",
                    Quantity = 6,          // vi phạm R9: không phải bội số của 4
                    UnitPrice = 100_000m,
                    DiscountPct = 15,      // vi phạm R6: Retail max 10%
                    Unit = "PALLET"
                },
                new OrderLine
                {
                    Sku = "B02",
                    Quantity = 1,
                    UnitPrice = 50_000m,
                    DiscountPct = 5,
                    Unit = "EA"
                }
            },
            Metadata = new Dictionary<string, string>()
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        Assert.False(result.IsValid);
        
        // Phải có ít nhất 2 lỗi: R6 và R9
        Assert.True(result.Errors.Count >= 2);
        
        // Kiểm tra có lỗi R6 cho B01
        Assert.Contains(result.Errors, e => e.Code == "R6" && e.Field == "B01");
        
        // Kiểm tra có lỗi R9 cho B01
        Assert.Contains(result.Errors, e => e.Code == "R9" && e.Field == "B01");
    }

    #endregion

    #region Test Case 3 — EDI thiếu metadata + không đủ ngày làm việc

    [Fact]
    public void Case3_EDI_MissingMetadata_And_InsufficientBusinessDays_ShouldFail()
    {
        // Arrange
        // Friday 28/03/2025 -> Monday 31/03/2025 = only 1 business day (Monday)
        // EDI requires >= 3 business days
        var friday = new DateTime(2025, 3, 28); // Thứ Sáu
        var monday = new DateTime(2025, 3, 31); // Thứ Hai

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST003",
            CustomerType = CustomerType.Wholesale,
            Channel = SalesChannel.EDI,
            SalesRepId = null,
            WarehouseCode = "WH01",
            OrderDate = friday,
            RequiredDate = monday,
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "C01",
                    Quantity = 4,
                    UnitPrice = 200_000m,
                    DiscountPct = 0,
                    Unit = "EA"
                }
            },
            Metadata = new Dictionary<string, string>
            {
                { "edi_sender_id", "X1" }
                // Thiếu edi_version
            }
        };

        // Act
        var result = _validator.Validate(order);

        // Assert
        Assert.False(result.IsValid);
        
        // Kiểm tra có lỗi R2 (không đủ ngày làm việc)
        Assert.Contains(result.Errors, e => e.Code == "R2");
        
        // Kiểm tra có lỗi R8 (thiếu edi_version)
        Assert.Contains(result.Errors, e => e.Code == "R8" && e.Field == "edi_version");
    }

    #endregion

    #region R1 Tests

    [Fact]
    public void R1_EmptyCustomerId_ShouldFail()
    {
        var order = CreateValidOrder();
        order.CustomerId = "";

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R1" && e.Field == "CustomerId");
    }

    [Fact]
    public void R1_EmptyWarehouseCode_ShouldFail()
    {
        var order = CreateValidOrder();
        order.WarehouseCode = null;

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R1" && e.Field == "WarehouseCode");
    }

    [Fact]
    public void R1_FutureOrderDate_ShouldFail()
    {
        var order = CreateValidOrder();
        order.OrderDate = DateTime.Today.AddDays(1);
        order.RequiredDate = DateTime.Today.AddDays(3);

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R1" && e.Field == "OrderDate");
    }

    #endregion

    #region R2 Tests

    [Fact]
    public void R2_Online_RequiredDateSameAsOrderDate_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.Online;
        order.SalesRepId = null;
        order.RequiredDate = order.OrderDate; // 0 days, need >= 1

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R2");
    }

    [Fact]
    public void R2_Offline_OneDay_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.Offline;
        order.SalesRepId = "SR01";
        order.RequiredDate = order.OrderDate.AddDays(1); // 1 day, need >= 2

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R2");
    }

    [Fact]
    public void R2_EDI_ThreeBusinessDays_ShouldPass()
    {
        // Monday + 3 business days = Thursday
        var monday = GetNextMonday();
        var order = CreateValidOrder();
        order.Channel = SalesChannel.EDI;
        order.OrderDate = monday;
        order.RequiredDate = monday.AddDays(3); // Thursday = 3 business days
        order.Metadata = new Dictionary<string, string>
        {
            { "edi_sender_id", "X1" },
            { "edi_version", "2.0" }
        };

        var result = _validator.Validate(order);

        Assert.DoesNotContain(result.Errors, e => e.Code == "R2");
    }

    #endregion

    #region R3 Tests

    [Fact]
    public void R3_Offline_NoSalesRep_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.Offline;
        order.SalesRepId = null;
        order.RequiredDate = order.OrderDate.AddDays(2);

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R3");
    }

    [Fact]
    public void R3_Online_WithSalesRep_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.Online;
        order.SalesRepId = "SR01"; // Should be null

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R3");
    }

    #endregion

    #region R4 Tests

    [Fact]
    public void R4_NoLines_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines = new List<OrderLine>();

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R4");
    }

    [Fact]
    public void R4_DuplicateSku_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines.Add(new OrderLine
        {
            Sku = order.Lines[0].Sku, // Duplicate
            Quantity = 1,
            UnitPrice = 100,
            DiscountPct = 0,
            Unit = "EA"
        });

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R4" && e.Message.Contains("trùng"));
    }

    #endregion

    #region R5 Tests

    [Fact]
    public void R5_ZeroQuantity_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines[0].Quantity = 0;

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R5" && e.Message.Contains("Quantity"));
    }

    [Fact]
    public void R5_InvalidUnit_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines[0].Unit = "INVALID";

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R5" && e.Message.Contains("Unit"));
    }

    [Fact]
    public void R5_NegativeDiscount_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines[0].DiscountPct = -5;

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R5" && e.Message.Contains("DiscountPct"));
    }

    #endregion

    #region R6 Tests

    [Fact]
    public void R6_Wholesale_DiscountOver30_ShouldFail()
    {
        var order = CreateValidOrder();
        order.CustomerType = CustomerType.Wholesale;
        order.Lines[0].DiscountPct = 35;

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R6");
    }

    [Fact]
    public void R6_Internal_AnyDiscount_ShouldPass()
    {
        var order = CreateValidOrder();
        order.CustomerType = CustomerType.Internal;
        order.Lines[0].DiscountPct = 90; // Very high discount

        var result = _validator.Validate(order);

        Assert.DoesNotContain(result.Errors, e => e.Code == "R6");
    }

    #endregion

    #region R7 Tests

    [Fact]
    public void R7_Retail_Over100M_ShouldFail()
    {
        var order = CreateValidOrder();
        order.CustomerType = CustomerType.Retail;
        order.Lines[0].Quantity = 1000;
        order.Lines[0].UnitPrice = 200_000m; // 200M > 100M limit

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R7");
    }

    [Fact]
    public void R7_Wholesale_Under2B_ShouldPass()
    {
        var order = CreateValidOrder();
        order.CustomerType = CustomerType.Wholesale;
        order.Lines[0].Quantity = 100;
        order.Lines[0].UnitPrice = 1_000_000m; // 100M < 2B limit

        var result = _validator.Validate(order);

        Assert.DoesNotContain(result.Errors, e => e.Code == "R7");
    }

    #endregion

    #region R8 Tests

    [Fact]
    public void R8_EDI_InvalidVersion_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.EDI;
        order.RequiredDate = order.OrderDate.AddDays(5);
        order.Metadata = new Dictionary<string, string>
        {
            { "edi_sender_id", "X1" },
            { "edi_version", "1.0" } // Invalid, should be 2.0 or 3.0
        };

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R8" && e.Field == "edi_version");
    }

    [Fact]
    public void R8_NonEDI_NoMetadata_ShouldPass()
    {
        var order = CreateValidOrder();
        order.Channel = SalesChannel.Online;
        order.SalesRepId = null;
        order.Metadata = null;

        var result = _validator.Validate(order);

        Assert.DoesNotContain(result.Errors, e => e.Code == "R8");
    }

    #endregion

    #region R9 Tests

    [Fact]
    public void R9_Pallet_Qty8_ShouldPass()
    {
        var order = CreateValidOrder();
        order.Lines[0].Unit = "PALLET";
        order.Lines[0].Quantity = 8;

        var result = _validator.Validate(order);

        Assert.DoesNotContain(result.Errors, e => e.Code == "R9");
    }

    [Fact]
    public void R9_Pallet_Qty5_ShouldFail()
    {
        var order = CreateValidOrder();
        order.Lines[0].Unit = "PALLET";
        order.Lines[0].Quantity = 5;

        var result = _validator.Validate(order);

        Assert.Contains(result.Errors, e => e.Code == "R9");
    }

    #endregion

    #region R10 - Collect All Errors

    [Fact]
    public void R10_MultipleErrors_ShouldCollectAll()
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "", // R1 error
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Offline,
            SalesRepId = null, // R3 error
            WarehouseCode = "", // R1 error
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1), // R2 error (Offline needs >= 2)
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "X01",
                    Quantity = 0, // R5 error
                    UnitPrice = -100, // R5 error
                    DiscountPct = 20, // R6 error (Retail max 10%)
                    Unit = "PALLET"
                }
            },
            Metadata = new Dictionary<string, string>()
        };

        var result = _validator.Validate(order);

        // Phải có nhiều lỗi từ nhiều rule khác nhau
        var errorCodes = result.Errors.Select(e => e.Code).Distinct().ToList();
        Assert.True(errorCodes.Count >= 4, $"Expected at least 4 different rule violations, got: {string.Join(", ", errorCodes)}");
    }

    #endregion

    #region Helpers

    private Order CreateValidOrder()
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = "CUST001",
            CustomerType = CustomerType.Retail,
            Channel = SalesChannel.Online,
            SalesRepId = null,
            WarehouseCode = "WH01",
            OrderDate = DateTime.Today,
            RequiredDate = DateTime.Today.AddDays(1),
            Lines = new List<OrderLine>
            {
                new OrderLine
                {
                    Sku = "SKU001",
                    Quantity = 1,
                    UnitPrice = 100_000m,
                    DiscountPct = 5,
                    Unit = "EA"
                }
            },
            Metadata = new Dictionary<string, string>()
        };
    }

    private DateTime GetNextMonday()
    {
        var today = DateTime.Today;
        int daysUntilMonday = ((int)DayOfWeek.Monday - (int)today.DayOfWeek + 7) % 7;
        if (daysUntilMonday == 0) daysUntilMonday = 7;
        return today.AddDays(-daysUntilMonday); // Previous Monday to ensure valid OrderDate
    }

    #endregion
}
