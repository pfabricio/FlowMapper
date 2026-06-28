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
| 🌿 **Flatten Mapping** | Map nested paths to flat properties (`Address.City.Name → CityName`) |
| 🧩 **Profile System** | Context-aware rules (Api, Domain, Integration) |
| ⚙️ **Fluent Configuration** | Global options via `AddFlowMapper(cfg => ...)` |
| 🚨 **Compile-Time Diagnostics** | Errors and warnings at build time (FM0001–FM0013) |
| 🔍 **IDE Support** | Real-time squiggles via Roslyn Analyzer |
| 📦 **Dependency Injection** | Built-in DI registration |
| 📊 **Benchmark Suite** | Compare against AutoMapper with BenchmarkDotNet |

---

## Quick Start

### 1. Install

```bash
dotnet add package FlowMapper
```

### 2. Define a mapper

```csharp
using FlowMapper;

[Map<User, UserDto>]
public partial class UserMapper;
```

### 3. Use it

```csharp
var mapper = new UserMapper();
UserDto dto = mapper.Map(user);
```

Or with DI:

```csharp
builder.Services.AddFlowMapper(cfg =>
{
    cfg.UseDefaultProfile("Api")
       .EnableFlatten()
       .PreferConstructor();
});

public class UserService(IMapper<User, UserDto> mapper)
{
    public UserDto Get(int id) => mapper.Map(_repository.Get(id));
}
```

---

## Profiles

FlowMapper supports different mapping behaviors per context:

| Profile | Flatten | Constructor | Strict |
|---|---|---|---|
| `Api` | ❌ | ❌ | ❌ |
| `Domain` | ✔ | ✔ | ✔ |
| `Integration` | ✔ | ❌ | ✔ |

```csharp
[Map<User, UserDto, Profile = "Domain")]
public partial class UserMapper;
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
