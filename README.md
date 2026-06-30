# 🚀 FlowMapper

**Compile-time object mapping framework for .NET — powered by Roslyn Source Generators.**

FlowMapper generates pure C# mapping code at compile time, eliminating runtime reflection, expression trees, and dynamic dispatch. The result: deterministic performance, zero startup cost, and compile-time validation.

---

## Features

| Feature | Description |
|---|---|
| ⚡ **Compile-Time Generation** | All mapping code is generated as `.g.cs` files during build |
| 🌳 **Nested Mapping** | Automatic deep mapping of complex object graphs |
| 🧱 **Constructor Mapping** | Records, immutable types, `init`-only properties |
| 🌿 **Flatten Mapping** | Map nested paths to flat properties (`Address.Street → AddressStreet`) |
| 🧩 **Profile System** | `MappingProfile` with `CreateMap<T1, T2>()` — the primary API |
| ⚙️ **Fluent API** | `ForMember`, `Ignore`, `UseConstructor`, `DisableFlatten`, `AfterMap`, `ConstructUsing` |
| 🚨 **Compile-Time Diagnostics** | Errors and warnings at build time (FM0001–FM0013) |
| 🔍 **IDE Support** | Real-time squiggles via Roslyn Analyzer |
| 📦 **Dependency Injection** | Built-in DI registration via `AddFlowMapper()` |
| 📊 **Benchmark Suite** | Compare against AutoMapper with BenchmarkDotNet |

---

## Quick Start

### 1. Install

```bash
dotnet add package FlowMapper
```

### 2. Define a profile

```csharp
using FlowMapper;

public class MyProfile : MappingProfile
{
    public MyProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
            .Ignore(dest => dest.InternalId)
            .AfterMap((source, target) => target.CreatedAt = DateTime.UtcNow);
    }
}
```

### 3. Use it

```csharp
var mapper = new UserMapper();
UserDto dto = mapper.Map(user);
```

Or with DI:

```csharp
builder.Services.AddFlowMapper();

public class UserService(IMapper<User, UserDto> mapper)
{
    public UserDto Get(int id) => mapper.Map(_repository.Get(id));
}
```

---

## Fluent API

Every method returns `MappingExpression<TSource, TDestination>` for chaining.

| Method | Description |
|--------|-------------|
| `ForMember(dest, opt)` | Customize how a destination property is mapped |
| `Ignore(dest)` | Skip a destination property |
| `UseConstructor()` | Prefer constructor matching (records, immutable types) |
| `DisableFlatten()` | Disable automatic flattening for this mapping |
| `AfterMap(expression)` | Execute logic after mapping (lambda) |
| `ConstructUsing(expression)` | Custom construction logic (lambda) |

```csharp
CreateMap<Order, OrderDto>()
    .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.Items.Sum(i => i.Price)))
    .ForMember(dest => dest.ShippingAddress, opt => opt.Ignore())
    .UseConstructor()
    .DisableFlatten()
    .AfterMap((source, target) => target.ProcessedAt = DateTime.UtcNow)
    .ConstructUsing(source => new OrderDto { Id = source.Id });
```

---

## Profiles

FlowMapper supports different mapping behaviors per context via `MappingProfile`:

| Profile | Flatten | Constructor | Strict |
|---|---|---|---|
| `Api` | ❌ | ❌ | ❌ |
| `Domain` | ✔ | ✔ | ✔ |
| `Integration` | ✔ | ❌ | ✔ |

```csharp
public class ApiProfile : MappingProfile
{
    public ApiProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

---

## Project Structure

```
src/
├── FlowMapper.Abstractions       # Attributes, interfaces, options
├── FlowMapper.Core               # Domain models (Flow, PropertyFlow, Policy)
├── FlowMapper.SourceGenerator    # Roslyn incremental generator
├── FlowMapper.Analyzers          # IDE-level diagnostics
├── FlowMapper.DependencyInjection# AddFlowMapper() extension
├── FlowMapper                    # Meta-package (entry point)
└── FlowMapper.Cli                # CLI tool (dotnet flowmapper)
```

---

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

---

## Diagnostics

| Code | Description | Severity |
|---|---|---|
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

---

## Architecture Principles

- **Compile-Time First** — decisions made at build, never at runtime
- **No Runtime Reflection** — pure C# code only
- **Flow Graph** — internal graph-based model drives all engines
- **Deterministic** — same input always produces same output
- **Explicit over Implicit** — ambiguity generates diagnostics, not guesses

---

## License

MIT
