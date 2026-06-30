global using AutoMapper;
global using BenchmarkDotNet.Attributes;
global using BenchmarkDotNet.Running;
global using FlowMapper.Abstractions;

BenchmarkRunner.Run<MapperBenchmarks>();

// --- Models ---

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int Age { get; set; }
}

public class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class Employee
{
    public string Name { get; set; } = string.Empty;
    public Address Address { get; set; } = new();
}

public class EmployeeDto
{
    public string Name { get; set; } = string.Empty;
    public string AddressStreet { get; set; } = string.Empty;
    public string AddressCity { get; set; } = string.Empty;
    public string AddressZipCode { get; set; } = string.Empty;
}

public record Product(int Id, string Name, decimal Price);
public record ProductDto(int Id, string Name, decimal Price);

public class Order
{
    public int Id { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}

public class OrderItem
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

public class OrderDto
{
    public int Id { get; set; }
    public int ItemCount { get; set; }
    public decimal Total { get; set; }
}

// --- Manual Mappers (hand-written, same as what FlowMapper generator would produce) ---

public class ManualUserMapper : IMapper<User, UserDto>
{
    public UserDto Map(User source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        Email = source.Email,
        Age = source.Age
    };
}

public class ManualEmployeeMapper : IMapper<Employee, EmployeeDto>
{
    public EmployeeDto Map(Employee source) => new()
    {
        Name = source.Name,
        AddressStreet = source.Address.Street,
        AddressCity = source.Address.City,
        AddressZipCode = source.Address.ZipCode
    };
}

public class ManualProductMapper : IMapper<Product, ProductDto>
{
    public ProductDto Map(Product source) => new(source.Id, source.Name, source.Price);
}

public class ManualOrderMapper : IMapper<Order, OrderDto>
{
    public OrderDto Map(Order source) => new()
    {
        Id = source.Id,
        ItemCount = source.Items.Count,
        Total = source.Items.Sum(i => i.Price * i.Quantity)
    };
}

// --- FlowMapper generated code equivalent (hand-written IMapper implementations) ---

public class FlowMapperBenchmark
{
    public static readonly ManualUserMapper UserMapper = new();
    public static readonly ManualEmployeeMapper EmployeeMapper = new();
    public static readonly ManualProductMapper ProductMapper = new();
    public static readonly ManualOrderMapper OrderMapper = new();
}

// --- AutoMapper Profile ---

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserDto>();

        CreateMap<Employee, EmployeeDto>()
            .ForMember(d => d.AddressStreet, o => o.MapFrom(s => s.Address.Street))
            .ForMember(d => d.AddressCity, o => o.MapFrom(s => s.Address.City))
            .ForMember(d => d.AddressZipCode, o => o.MapFrom(s => s.Address.ZipCode));

        CreateMap<Product, ProductDto>();

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.ItemCount, o => o.MapFrom(s => s.Items.Count))
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Items.Sum(i => i.Price * i.Quantity)));
    }
}

// --- Benchmark ---

[MemoryDiagnoser]
public class MapperBenchmarks
{
    private IMapper _autoMapper = null!;

    private User _user = null!;
    private Employee _employee = null!;
    private Product _product = null!;
    private Order _order = null!;

    [GlobalSetup]
    public void Setup()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
        _autoMapper = config.CreateMapper();

        _user = new User { Id = 1, Name = "Alice", Email = "alice@example.com", Age = 30 };
        _employee = new Employee { Name = "Bob", Address = new Address { Street = "123 Main St", City = "Springfield", ZipCode = "12345" } };
        _product = new Product(1, "Widget", 9.99m);
        _order = new Order
        {
            Id = 42,
            Items = [new OrderItem { Name = "A", Price = 10m, Quantity = 2 }, new OrderItem { Name = "B", Price = 5m, Quantity = 3 }]
        };
    }

    // --- Simple Flat ---

    [Benchmark]
    public UserDto Manual_SimpleFlat()
    {
        return new UserDto { Id = _user.Id, Name = _user.Name, Email = _user.Email, Age = _user.Age };
    }

    [Benchmark]
    public UserDto FlowMapper_SimpleFlat()
    {
        return FlowMapperBenchmark.UserMapper.Map(_user);
    }

    [Benchmark]
    public UserDto AutoMapper_SimpleFlat()
    {
        return _autoMapper.Map<UserDto>(_user);
    }

    // --- Flatten ---

    [Benchmark]
    public EmployeeDto Manual_Flatten()
    {
        return new EmployeeDto
        {
            Name = _employee.Name,
            AddressStreet = _employee.Address.Street,
            AddressCity = _employee.Address.City,
            AddressZipCode = _employee.Address.ZipCode
        };
    }

    [Benchmark]
    public EmployeeDto FlowMapper_Flatten()
    {
        return FlowMapperBenchmark.EmployeeMapper.Map(_employee);
    }

    [Benchmark]
    public EmployeeDto AutoMapper_Flatten()
    {
        return _autoMapper.Map<EmployeeDto>(_employee);
    }

    // --- Constructor (records) ---

    [Benchmark]
    public ProductDto Manual_Constructor()
    {
        return new ProductDto(_product.Id, _product.Name, _product.Price);
    }

    [Benchmark]
    public ProductDto FlowMapper_Constructor()
    {
        return FlowMapperBenchmark.ProductMapper.Map(_product);
    }

    [Benchmark]
    public ProductDto AutoMapper_Constructor()
    {
        return _autoMapper.Map<ProductDto>(_product);
    }

    // --- Collection with computed properties ---

    [Benchmark]
    public OrderDto Manual_Collection()
    {
        return new OrderDto
        {
            Id = _order.Id,
            ItemCount = _order.Items.Count,
            Total = _order.Items.Sum(i => i.Price * i.Quantity)
        };
    }

    [Benchmark]
    public OrderDto FlowMapper_Collection()
    {
        return FlowMapperBenchmark.OrderMapper.Map(_order);
    }

    [Benchmark]
    public OrderDto AutoMapper_Collection()
    {
        return _autoMapper.Map<OrderDto>(_order);
    }
}
