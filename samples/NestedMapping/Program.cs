using FlowMapper.Abstractions;

Console.WriteLine("=== Nested Mapping ===");
Console.WriteLine("Maps complex nested types recursively:");
Console.WriteLine("  Order.Customer.Name -> OrderDto.CustomerDto.Name");
Console.WriteLine();

var mapper = new OrderToOrderDtoMapper();
var order = new Order
{
    Id = 1,
    Total = 99.99m,
    Customer = new Customer
    {
        Name = "Charlie",
        Email = "charlie@example.com",
        Address = new Address
        {
            Street = "456 Oak Ave",
            City = "Portland"
        }
    }
};
var dto = mapper.Map(order);

Console.WriteLine($"OrderDto: Id={dto.Id}, Total={dto.Total}");
Console.WriteLine($"  Customer: {dto.Customer.Name} ({dto.Customer.Email})");
Console.WriteLine($"    Address: {dto.Customer.Address.Street}, {dto.Customer.Address.City}");

// --- Manual nested mapper (normally generated) ---
// FlowMapper generates MapXxx() helper methods for each nested type.

public class OrderToOrderDtoMapper : IMapper<Order, OrderDto>
{
    public OrderDto Map(Order source) => new()
    {
        Id = source.Id,
        Total = source.Total,
        Customer = MapCustomer(source.Customer)
    };

    private CustomerDto MapCustomer(Customer source) => new()
    {
        Name = source.Name,
        Email = source.Email,
        Address = MapAddress(source.Address)
    };

    private AddressDto MapAddress(Address source) => new()
    {
        Street = source.Street,
        City = source.City
    };
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class Customer
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public Customer Customer { get; set; } = new();
}

public class AddressDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}

public class CustomerDto
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public AddressDto Address { get; set; } = new();
}

public class OrderDto
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public CustomerDto Customer { get; set; } = new();
}
