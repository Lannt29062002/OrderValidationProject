using System.Text;
using OrderValidation.Core.Models;
using OrderValidation.Core.Validation;
using OrderValidation.Demo;

// Configure Vietnamese encoding
Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

var app = new Application(new OrderValidator());
app.Run();

/// <summary>
/// Main application class
/// </summary>
public class Application
{
    private readonly IOrderValidator _validator;

    public Application(IOrderValidator validator)
    {
        _validator = validator;
    }

    public void Run()
    {
        while (true)
        {
            ShowMenu();
            var choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    InputAndValidateOrder();
                    break;
                case "2":
                    RunAllTestCases();
                    break;
                case "0":
                    Console.WriteLine("Tạm biệt!");
                    return;
                default:
                    Console.WriteLine("Lựa chọn không hợp lệ!");
                    break;
            }

            Console.WriteLine("\nNhấn phím bất kỳ để tiếp tục...");
            Console.ReadKey();
        }
    }

    private void ShowMenu()
    {
        Console.Clear();
        Console.WriteLine("╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           HỆ THỐNG VALIDATE ĐƠN HÀNG                         ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  1. Nhập đơn hàng mới                                        ║");
        Console.WriteLine("║  2. Chạy test cases (R1-R10)                                 ║");
        Console.WriteLine("║  0. Thoát                                                    ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.Write("\nChọn chức năng: ");
    }

    private void RunAllTestCases()
    {
        Console.Clear();
        Console.WriteLine("=== CHẠY TEST CASES (R1-R10) ===\n");

        string currentRule = "";
        foreach (var (name, description, order) in TestCases.GetAllTestCases())
        {
            // Print rule header when rule changes
            string rule = name.Split('.')[0];
            if (rule != currentRule)
            {
                currentRule = rule;
                PrintRuleHeader(rule);
            }

            Console.WriteLine($"\n--- {name}: {description} ---");
            PrintResult(_validator.Validate(order));
        }

        Console.WriteLine("\n══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    HOÀN THÀNH TEST R1-R10");
        Console.WriteLine("══════════════════════════════════════════════════════════════");
    }

    private void PrintRuleHeader(string rule)
    {
        var descriptions = new Dictionary<string, string>
        {
            ["R1"] = "CustomerId, WarehouseCode không null/empty, OrderDate không tương lai",
            ["R2"] = "RequiredDate tối thiểu: Online ≥1 ngày, Offline ≥2 ngày, EDI ≥3 ngày làm việc",
            ["R3"] = "Offline bắt buộc SalesRepId, Online phải null",
            ["R4"] = "Lines 1-50 dòng, SKU không trùng, SKU không null/empty",
            ["R5"] = "Quantity > 0, UnitPrice > 0, DiscountPct [0-100], Unit hợp lệ",
            ["R6"] = "Chiết khấu tối đa: Retail ≤10%, Wholesale ≤30%, Internal không giới hạn",
            ["R7"] = "Tổng giá trị: Retail ≤100M, Wholesale ≤2B, Internal không giới hạn",
            ["R8"] = "EDI cần edi_sender_id và edi_version (2.0 hoặc 3.0)",
            ["R9"] = "Unit = PALLET thì Quantity phải là bội số của 4",
            ["R10"] = "Collect ALL errors - không dừng sớm"
        };

        Console.WriteLine($"\n══════════════════════════════════════════════════════════════");
        Console.WriteLine($"{rule}: {descriptions.GetValueOrDefault(rule, "")}");
        Console.WriteLine("══════════════════════════════════════════════════════════════");
    }

    private void InputAndValidateOrder()
    {
        Console.Clear();
        Console.WriteLine("=== NHẬP THÔNG TIN ĐƠN HÀNG ===\n");

        var order = new Order
        {
            Id = Guid.NewGuid(),
            Lines = new List<OrderLine>(),
            Metadata = new Dictionary<string, string>()
        };

        // Customer Info
        order.CustomerId = Prompt("Customer ID");
        order.CustomerType = PromptEnum<CustomerType>("Loại khách hàng", "1.Retail  2.Wholesale  3.Internal");
        order.Channel = PromptEnum<SalesChannel>("Kênh bán hàng", "1.Online  2.Offline  3.EDI");
        order.WarehouseCode = Prompt("Warehouse Code");

        // Sales Rep (for Offline)
        if (order.Channel == SalesChannel.Offline)
            order.SalesRepId = Prompt("Sales Rep ID");

        // Dates
        order.OrderDate = PromptDate("Order Date (dd/MM/yyyy, Enter = hôm nay)", DateTime.Today);
        order.RequiredDate = PromptDate("Required Date (dd/MM/yyyy)", order.OrderDate.AddDays(3));

        // EDI Metadata
        if (order.Channel == SalesChannel.EDI)
        {
            var senderId = Prompt("EDI Sender ID");
            if (!string.IsNullOrWhiteSpace(senderId))
                order.Metadata["edi_sender_id"] = senderId;

            var version = Prompt("EDI Version (2.0/3.0)");
            if (!string.IsNullOrWhiteSpace(version))
                order.Metadata["edi_version"] = version;
        }

        // Order Lines
        Console.WriteLine("\n=== NHẬP DÒNG SẢN PHẨM ===");
        Console.WriteLine("(Nhập SKU trống để kết thúc)\n");

        int lineNo = 1;
        while (true)
        {
            Console.WriteLine($"--- Dòng {lineNo} ---");
            var sku = Prompt("SKU");
            if (string.IsNullOrWhiteSpace(sku)) break;

            var line = new OrderLine
            {
                Sku = sku,
                Quantity = PromptInt("Số lượng"),
                UnitPrice = PromptDecimal("Đơn giá"),
                DiscountPct = PromptDecimal("Chiết khấu (%)"),
                Unit = PromptUnit()
            };

            order.Lines.Add(line);
            lineNo++;
            Console.WriteLine();
        }

        // Validate and show result
        Console.WriteLine("\n" + new string('=', 50));
        Console.WriteLine("KẾT QUẢ VALIDATE");
        Console.WriteLine(new string('=', 50));

        var result = _validator.Validate(order);
        PrintResult(result, showTotal: true, order: order);
    }

    private void PrintResult(ValidationResult result, bool showTotal = false, Order? order = null)
    {
        if (result.IsValid)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("IsValid: True");
            Console.ResetColor();

            if (showTotal && order?.Lines != null)
            {
                decimal total = order.Lines.Sum(l => l.Quantity * l.UnitPrice * (1 - l.DiscountPct / 100));
                Console.WriteLine($"Tổng giá trị đơn hàng: {total:N0} đ");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"IsValid: False - Errors ({result.Errors.Count}):");
            Console.ResetColor();

            foreach (var error in result.Errors)
            {
                Console.WriteLine($"  [{error.Code}] {error.Message} (Field: {error.Field})");
            }
        }
    }

    #region Input Helpers

    private string Prompt(string label)
    {
        Console.Write($"{label}: ");
        return Console.ReadLine() ?? "";
    }

    private T PromptEnum<T>(string label, string options) where T : struct, Enum
    {
        Console.WriteLine($"\n{label}:");
        Console.WriteLine($"  {options}");
        Console.Write("Chọn: ");
        var input = Console.ReadLine();

        var values = Enum.GetValues<T>();
        if (int.TryParse(input, out int index) && index >= 1 && index <= values.Length)
            return values[index - 1];

        return values[0];
    }

    private DateTime PromptDate(string label, DateTime defaultValue)
    {
        Console.Write($"{label}: ");
        var input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
            return defaultValue;

        if (DateTime.TryParseExact(input, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        return defaultValue;
    }

    private int PromptInt(string label)
    {
        Console.Write($"{label}: ");
        return int.TryParse(Console.ReadLine(), out int value) ? value : 0;
    }

    private decimal PromptDecimal(string label)
    {
        Console.Write($"{label}: ");
        return decimal.TryParse(Console.ReadLine(), out decimal value) ? value : 0;
    }

    private string PromptUnit()
    {
        Console.WriteLine("Đơn vị: 1.EA  2.CASE  3.PALLET");
        Console.Write("Chọn: ");
        return Console.ReadLine() switch
        {
            "2" => "CASE",
            "3" => "PALLET",
            _ => "EA"
        };
    }

    #endregion
}
