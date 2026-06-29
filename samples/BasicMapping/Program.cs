using FlowMapper.Abstractions;

Console.WriteLine("=== Basic Mapping ===");

// Normally, FlowMapper SourceGenerator produces the mapper implementation at compile time:
//
//   public class BasicMappingProfile : ProfileDefinition
//   {
//       public BasicMappingProfile()
//       {
//           CreateMap<User, UserDto>();
//       }
//   }
//
// The generator emits a .g.cs file with the Map() method body.

var mapper = new UserToUserDtoMapper();
var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
var dto = mapper.Map(user);

Console.WriteLine($"UserDto: Id={dto.Id}, Name={dto.Name}, Email={dto.Email}");

Console.WriteLine();
Console.WriteLine("=== AfterMap with Lambda Inline ===");
Console.WriteLine("ProfileDefinition now supports strongly-typed callbacks:");
Console.WriteLine("  CreateMap<Order, OrderDto>()");
Console.WriteLine("      .AfterMap((source, target) => target.FullName = source.FirstName + ' ' + source.LastName)");
Console.WriteLine();

var orderMapper = new OrderToOrderDtoMapper();
var order = new Order { Id = 1, FirstName = "John", LastName = "Doe", Amount = 100 };
var orderDto = orderMapper.Map(order);
Console.WriteLine($"OrderDto: Id={orderDto.Id}, FullName={orderDto.FullName}, Amount={orderDto.Amount}");

// --- Manual mapper (normally generated) ---

public class UserToUserDtoMapper : IMapper<User, UserDto>
{
    public UserDto Map(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email
    };
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

// --- Order with AfterMap lambda ---
// Equivalent to what the source generator produces from:
//   CreateMap<Order, OrderDto>().AfterMap((source, target) => target.FullName = source.FirstName + " " + source.LastName)

public class OrderToOrderDtoMapper : IMapper<Order, OrderDto>
{
    public OrderDto Map(Order source)
    {
        var target = new OrderDto
        {
            Id = source.Id,
            Amount = source.Amount
        };
        target.FullName = source.FirstName + " " + source.LastName;
        return target;
    }
}

public class Order
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}
