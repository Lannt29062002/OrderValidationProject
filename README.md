# Order Validation System

Hệ thống validate đơn hàng theo nghiệp vụ phân phối hàng tiêu dùng.

## Yêu cầu

- .NET 8.0 SDK hoặc cao hơn
- Visual Studio 2022 / VS Code / Rider

## Cấu trúc Project

```
OrderValidation/
├── src/
│   ├── OrderValidation.Core/        # Core library - Business logic
│   │   ├── Models/                  # Data models (Order, OrderLine, Enums)
│   │   └── Validation/
│   │       ├── Rules/               # 9 validation rules (R1-R9)
│   │       ├── IOrderValidator.cs   # Interface
│   │       ├── OrderValidator.cs    # Main validator
│   │       └── ValidationResult.cs  # Result & Error classes
│   └── OrderValidation.Demo/        # Console demo application
│       ├── Program.cs
│       └── TestCases.cs             # Test data
└── tests/
    └── OrderValidation.Tests/       # Unit tests (25 tests)
```

## Cài đặt

```bash
# Clone repository
git clone https://github.com/Lannt29062002/OrderValidationProject.git
cd OrderValidationProject

# Restore packages
dotnet restore

# Build solution
dotnet build
```

## Chạy ứng dụng

### 1. Chạy Demo Console

```bash
dotnet run --project src/OrderValidation.Demo
```

Hoặc trong Visual Studio:
1. Set **OrderValidation.Demo** là Startup Project
2. Nhấn **Ctrl+F5** (Run without debugging)

### 2. Chạy Unit Tests

```bash
dotnet test
```

Hoặc trong Visual Studio: **Test** → **Run All Tests** (Ctrl+R, A)

## Business Rules

| Rule | Mô tả |
|------|-------|
| **R1** | `CustomerId`, `WarehouseCode` không null/empty. `OrderDate` không được là ngày tương lai |
| **R2** | `RequiredDate` sau `OrderDate` tối thiểu: Online ≥1 ngày, Offline ≥2 ngày, EDI ≥3 ngày làm việc |
| **R3** | Kênh Offline bắt buộc `SalesRepId`. Kênh Online `SalesRepId` phải null |
| **R4** | `Lines` từ 1-50 dòng. Không có SKU trùng. SKU không null/empty |
| **R5** | Mỗi dòng: `Quantity > 0`, `UnitPrice > 0`, `DiscountPct` trong [0,100], `Unit` ∈ {EA, CASE, PALLET} |
| **R6** | Chiết khấu tối đa: Retail ≤10%, Wholesale ≤30%, Internal không giới hạn |
| **R7** | Tổng giá trị: Retail ≤100M, Wholesale ≤2B, Internal không giới hạn |
| **R8** | Kênh EDI bắt buộc `edi_sender_id` và `edi_version` (2.0 hoặc 3.0) trong Metadata |
| **R9** | `Unit = "PALLET"` thì `Quantity` phải là bội số của 4 |
| **R10** | Collect ALL errors - không dừng ở lỗi đầu tiên |

## Sử dụng trong code

```csharp
using OrderValidation.Core.Models;
using OrderValidation.Core.Validation;

// Tạo validator
var validator = new OrderValidator();

// Tạo đơn hàng
var order = new Order
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
        new() { Sku = "A01", Quantity = 2, UnitPrice = 500_000m, DiscountPct = 10, Unit = "EA" }
    },
    Metadata = new Dictionary<string, string>()
};

// Validate
var result = validator.Validate(order);

if (result.IsValid)
{
    Console.WriteLine("Đơn hàng hợp lệ!");
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"[{error.Code}] {error.Message} (Field: {error.Field})");
    }
}
```

## Thiết kế

- **Open/Closed Principle**: Mỗi rule là một class riêng implement `IOrderValidationRule`
- **Dependency Injection Ready**: `OrderValidator` nhận `IEnumerable<IOrderValidationRule>` qua constructor
- **Extensible**: Thêm rule mới không cần sửa core logic

## Test Coverage

- 25 unit tests covering tất cả R1-R10
- Edge cases: null handling, boundary values
- Integration test: multiple errors collection

## License

MIT
