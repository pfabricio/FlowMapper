using FlowMapper.Abstractions;

Console.WriteLine("=== Profile System ===");
Console.WriteLine("Profiles group related mappings with shared policy settings:");
Console.WriteLine("  Api       - strict: false, flatten: false, constructor: false");
Console.WriteLine("  Domain    - strict: true,  flatten: true,  constructor: true");
Console.WriteLine("  Internal  - strict: false, flatten: true,  constructor: false");
Console.WriteLine();

// Without a real source generator, we demonstrate the equivalent mapping logic.
var mapper = new UserToApiDtoMapper();
var user = new User
{
    Id = 1,
    Name = "Eve",
    Email = "eve@example.com",
    Internal = "secret-token-123"
};

var apiDto = mapper.Map(user);
Console.WriteLine($"ApiDto:       Id={apiDto.Id}, Name={apiDto.Name}");
// API profile: Internal field is NOT mapped (excluded from API responses)

Console.WriteLine();
Console.WriteLine("=== AfterMap Callback (method group) ===");
Console.WriteLine("Normally the source generator would produce:");
Console.WriteLine("  public class OrderMappingProfile : ProfileDefinition");
Console.WriteLine("  {");
Console.WriteLine("      public OrderMappingProfile()");
Console.WriteLine("      {");
Console.WriteLine("          CreateMap<Order, OrderDto>()");
Console.WriteLine("              .AfterMap(CalculateTotals);");
Console.WriteLine("      }");
Console.WriteLine("      private void CalculateTotals(Order source, OrderDto target)");
Console.WriteLine("      {");
Console.WriteLine("          target.Total = source.Price * source.Quantity;");
Console.WriteLine("      }");
Console.WriteLine("  }");
Console.WriteLine();

// Demonstrate the equivalent runtime behavior:
var orderMapper = new OrderToOrderDtoMapper();
var order = new Order { Id = 1, Price = 29.99m, Quantity = 3 };
var orderDto = orderMapper.Map(order);
Console.WriteLine($"OrderDto: Id={orderDto.Id}, Subtotal={orderDto.Subtotal}, Total={orderDto.Total}");

// --- Profile-aware mapper (normally generated) ---
// With ProfileDefinition and CreateMap<User, UserDto>().

public class UserToApiDtoMapper : IMapper<User, ApiUserDto>
{
    public ApiUserDto Map(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email
        // Internal intentionally excluded — API profile strips internal data
    };
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Internal { get; set; } = string.Empty;
}

public class ApiUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// --- Order mapping with AfterMap callback (method group) ---
// Equivalent to what the source generator produces from:
//   CreateMap<Order, OrderDto>().AfterMap(CalculateTotals);

public class OrderToOrderDtoMapper : IMapper<Order, OrderDto>
{
    public OrderDto Map(Order source)
    {
        var target = new OrderDto
        {
            Id = source.Id,
            Subtotal = source.Price * source.Quantity
        };
        CalculateTotals(source, target);
        return target;
    }

    private static void CalculateTotals(Order source, OrderDto target)
    {
        target.Total = target.Subtotal * 1.1m; // 10% tax
    }
}

public class Order
{
    public int Id { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
}
