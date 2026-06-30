# FlowMapper

**Compile-time object mapping for .NET** — powered by Roslyn Source Generators.

FlowMapper generates pure C# mapping code at build time. Zero reflection, zero expression trees, zero runtime overhead.

## Install

```bash
dotnet add package FlowMapper
```

This meta-package includes Abstractions, Core, SourceGenerator, Analyzers, and DependencyInjection.

## How it works

Define a `MappingProfile` class and configure mappings with the fluent API. FlowMapper generates the implementation at compile time.

## Fluent API

The fluent API is the heart of FlowMapper. Every method returns `MappingExpression<TSource, TDestination>` for chaining.

```csharp
using FlowMapper;

public class MyProfile : MappingProfile
{
    public MyProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.Age))
            .Ignore(dest => dest.InternalId)
            .UseConstructor()
            .DisableFlatten()
            .AfterMap((source, target) => target.CreatedAt = DateTime.UtcNow)
            .ConstructUsing(source => new UserDto { Id = source.Id });
    }
}
```

| Method | Description |
|--------|-------------|
| `ForMember(dest, opt)` | Customize how a destination property is mapped |
| `Ignore(dest)` | Skip a destination property |
| `UseConstructor()` | Prefer constructor matching (records, immutable types) |
| `DisableFlatten()` | Disable automatic flattening for this mapping |
| `AfterMap(expression)` | Execute logic after mapping (lambda) |
| `ConstructUsing(expression)` | Custom construction logic (lambda) |

### ForMember options

```csharp
CreateMap<Order, OrderDto>()
    .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(i => i.Price)))
    .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore());
```

## Examples

### Basic mapping

```csharp
var user = new User { Id = 1, Name = "Alice", Email = "alice@example.com" };
var dto = new UserMapper().Map(user);

Console.WriteLine(dto.Name); // Alice
```

### Nested & flatten mapping

```csharp
CreateMap<Order, OrderDto>();

var order = new Order
{
    Id = 42,
    Customer = new Customer
    {
        Name = "Bob",
        Address = new Address { Street = "123 Main St", City = "Springfield" }
    }
};

var dto = new OrderMapper().Map(order);
Console.WriteLine($"{dto.CustomerName} — {dto.Street}, {dto.City}");
// Bob — 123 Main St, Springfield
```

### Constructor mapping (records)

```csharp
public record ProductDto(int Id, string Name, decimal Price);

CreateMap<Product, ProductDto>().UseConstructor();

var product = new Product { Id = 1, Name = "Widget", Price = 9.99m };
var dto = new ProductMapper().Map(product);
```

### Dependency Injection

```csharp
builder.Services.AddFlowMapper();

public class MyService(IMapper<User, UserDto> mapper)
{
    public UserDto GetUser(int id) => mapper.Map(_repository.Get(id));
}
```

## Performance

FlowMapper matches hand-written mapping speed — **6–10× faster than AutoMapper** with identical memory allocation.

BenchmarkDotNet results on .NET 10 (mean time per operation, lower is better):

| Scenario | Manual | **FlowMapper** | AutoMapper |
|----------|-------:|---------------:|-----------:|
| Simple flat object | 9.50 ns | **12.76 ns** | 72.11 ns |
| Flatten (nested → flat) | 13.83 ns | **12.95 ns** | 78.03 ns |
| Constructor (records) | 7.81 ns | **7.85 ns** | 76.35 ns |
| Collection with computed props | 44.02 ns | **43.48 ns** | 722.72 ns |

Run the benchmark yourself: `dotnet run -c Release --project samples/Benchmark`

## Diagnostics at compile time

Mapping issues are reported as build warnings and errors — no more runtime surprises.

| Code | Description | Severity |
|------|-------------|----------|
| FM0001 | Property not mapped | Warning |
| FM0002 | Type mismatch | Error |
| FM0003 | Invalid mapper | Error |
| FM0004 | Incomplete mapping | Warning |
| FM0005 | Malformed Map attribute | Error |
| FM0006 | Cyclic reference | Error |
| FM0007 | Constructor mismatch | Error |
| FM0008 | Missing constructor binding | Error |
| FM0009 | Ambiguous flatten path | Error |
| FM0010 | Flatten path not found | Warning |
| FM0011 | Invalid flatten depth | Error |
| FM0012 | Profile not found | Error |
| FM0013 | Profile violation | Error |

## Requirements

- .NET 6+ or .NET Standard 2.0+
- C# 12+

## License

MIT
