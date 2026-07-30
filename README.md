# FlowMapper V2

[![NuGet](https://img.shields.io/nuget/v/FlowMapper.svg?style=flat-square)](https://www.nuget.org/packages/FlowMapper)

**FlowMapper is a compile-time data mapping platform for .NET.**

It combines object-object mapping, micro-ORM, deserialization, source generation, and execution pipelines under a unified architecture focused on **performance**, **extensibility**, and **zero runtime reflection**.

![FlowMapper Overview](images/Image%2023%20de%20jul.%20de%202026%2C%2016_52_15.png)

```csharp
var dto = flow.Map<User, UserDto>(user);                    // Object → Object
var rows = await flow.QueryAsync<CustomerDto>(sql);         // SQL → DTO
var json = flow.FromJson<CustomerDto>(jsonString);           // JSON → DTO
```

---

## Architecture

```
           SQL / JSON / XML / Object
                     │
                     ▼
         ┌───────────────────────┐
         │   Compiler Pipeline   │
         │  (13 optimization passes) │
         └───────────┬───────────┘
                     │
                     ▼
         ┌───────────────────────┐
         │   Execution Plan      │
         │   (Materialization,   │
         │    Mapping, SQL Artifacts)│
         └───────────┬───────────┘
                     │
         ┌───────────┴───────────┐
         ▼                       ▼
   Mapping Pipeline     Materialization Pipeline
   (Object → Object)    (DataReader → Object)
         │                       │
         ▼                       ▼
   Runtime Engine ────────── Execution Scope
         │
         ▼
     DTO / Entity
```

---

## Why FlowMapper?

| Benefit | Description |
|---------|-------------|
| ✅ **Compile-time Mapping** | Source generator produces `IMapper<,>` code at build time |
| ✅ **Zero Runtime Reflection** | No `System.Reflection` in hot paths — faster startup & execution |
| ✅ **Source Generator** | Roslyn `IIncrementalGenerator` — errors show at compile time, not runtime |
| ✅ **Native AOT Ready** | No dynamic code generation — works with `nativeaot` |
| ✅ **Nested Mapping** | Recursive object-to-object and SQL-to-DTO with aliases |
| ✅ **Flatten Mapping** | Auto-flatten `Address.Street` → `AddressStreet` with `_` separator |
| ✅ **Micro-ORM** | `QueryAsync<T>`, `StreamAsync<T>`, `CommandAsync<T>` with cascade materialization |
| ✅ **4 SQL Providers** | SQL Server, PostgreSQL, MySQL, Oracle — each with dialect-aware pagination |
| ✅ **Execution Pipelines** | Middleware-based: `IPipelineBehavior` chain for cross-cutting concerns |
| ✅ **Materialization Pipeline** | Caching, conversion, and null-handling middlewares |
| ✅ **Validation Pipeline** | Rule-based validation with `IValidationRule` |
| ✅ **Full-Text Search** | `SearchFtsAsync<T>` with dialect-aware FTS condition injection across 4 providers |
| ✅ **Runtime Diagnostics** | 6 built-in rules (FTS index, LIKE wildcard, ORDER BY index, SELECT *, large OFFSET, Cartesian JOIN) |
| ✅ **Diagnostics Pipeline** | Event and middleware-based diagnostics with metrics |
| ✅ **Schema Inspection** | Application-lifetime cache with `ISchemaInspector` for verifying FTS indexes against the database |
| ✅ **Diagnostic Telemetry** | Per-code counters and `OnDiagnostic` event for OpenTelemetry integration |
| ✅ **Compile-time FTS Analysis** | Source generator emits FM5001/FM5002 warnings for misconfigured FTS profiles |
| ✅ **Compiler Pipeline** | 13 optimization passes (flatten, fusion, constant eval, dead metadata elimination) |
| ✅ **Plugin SDK** | Extend everything: providers, stages, passes, rules, generators |
| ✅ **Deserialization** | JSON, XML, TXT/CSV — all with nested DTO support |
| ✅ **Caching** | 5 levels: external `ICacheProvider`, compiled delegates, flows, plans |

---

## Comparison

| Feature | FlowMapper | AutoMapper | Mapster | Dapper |
|---------|-----------|------------|---------|--------|
| Compile-time Mapping | ✅ | ❌ | ✅ | ❌ |
| Source Generator | ✅ | ❌ | ✅ | ❌ |
| Nested Mapping | ✅ | ✅ | ✅ | ❌ |
| Flatten Mapping | ✅ | ✅ | ✅ | ❌ |
| Micro-ORM | ✅ | ❌ | ❌ | ✅ |
| SQL Providers (4) | ✅ | ❌ | ❌ | ✅ |
| Materialization Pipeline | ✅ | ❌ | ❌ | Partial |
| Execution Plans | ✅ | ❌ | ❌ | ❌ |
| Plugin SDK | ✅ | ❌ | ❌ | ❌ |
| Diagnostics Pipeline | ✅ | ❌ | ❌ | ❌ |
| Validation Pipeline | ✅ | ❌ | ❌ | ❌ |
| Deserialization (JSON/XML/TXT) | ✅ | ❌ | ❌ | ❌ |
| Native AOT | ✅ | ❌ | ❌ | ❌ |

---

## Installation

```xml
<PackageReference Include="FlowMapper" Version="2.1.0" />
```

Requires .NET 8.0+

---

## Quick Start

```csharp
// Register FlowMapper with defaults
services.AddFlowMapper();

// Inject and use
public class MyService
{
    private readonly IFlowMapper _flow;
    public MyService(IFlowMapper flow) { _flow = flow; }

    public async Task Execute()
    {
        // Object → Object
        var dto = _flow.Map<User, UserDto>(new User { Id = 1, Name = "John", Email = "john@email.com" });

        // SQL → DTO with nested materialization via aliases
        var customers = await _flow.QueryAsync<CustomerDto>(@"
            SELECT u.Id, u.Name,
                   p.Id   AS Profile_Id,
                   p.Name AS Profile_Name
            FROM Users u
            LEFT JOIN Profiles p ON p.UserId = u.Id");

        // JSON → DTO (nested)
        var json = """{ "Id": 1, "Name": "Maria", "Profile": { "Id": 10, "Name": "Admin" } }""";
        var jDto = _flow.FromJson<CustomerDto>(json);

        // XML → DTO (nested)
        var xml = """<CustomerDto><Id>1</Id><Name>Peter</Name><Profile><Id>10</Id><Name>Support</Name></Profile></CustomerDto>""";
        var xDto = _flow.FromXml<CustomerDto>(xml);

        // CSV → DTO
        var csv = new[] { "Id;Name;Email", "1;John;john@email.com", "2;Maria;maria@email.com" };
        var list = _flow.FromText<UserDto>(csv, TextDelimiter.Semicolon, hasHeader: true);
    }
}
```

---

## Features

### Object-Object Mapping (AutoMapper-like)

```csharp
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.Name} ({s.Email})"))
            .ReverseMap();

        CreateMap<Customer, CustomerDto>()
            .ForPath(d => d.Profile.Name, opt => opt.MapFrom(s => s.ProfileName));
    }
}

var dto = _flow.Map<User, UserDto>(user);
```

### SQL → DTO with Cascade Nested Materialization

Auto-maps flat SQL resultsets into nested DTOs using column aliases and property-driven grouping.

```csharp
class CustomerDto {
    public int Id { get; set; }
    public string Name { get; set; }
    public ProfileDto Profile { get; set; }  // ← nested
}
class ProfileDto {
    public int Id { get; set; }
    public string Name { get; set; }
}
```

```sql
SELECT u.Id, u.Name,
       p.Id   AS Profile_Id,   -- ← prefix "Profile_"
       p.Name AS Profile_Name  -- ← prefix "Profile_"
FROM Users u
LEFT JOIN Profiles p ON p.UserId = u.Id
```

```csharp
var customers = await _flow.QueryAsync<CustomerDto>(sql);
// CustomerDto.Profile is auto-populated for each row
```

### JSON → DTO (Nested)

```csharp
var json = """{ "Id": 1, "Name": "Maria", "Profile": { "Id": 10, "Name": "Admin" } }""";
var dto = _flow.FromJson<CustomerDto>(json);
// dto.Profile.Name == "Admin"
```

### XML → DTO (Nested)

```csharp
var xml = """<CustomerDto><Id>1</Id><Name>Peter</Name><Profile><Id>10</Id><Name>Support</Name></Profile></CustomerDto>""";
var dto = _flow.FromXml<CustomerDto>(xml);
```

### TXT → DTO (Flat)

**With header** — matches column names to property names (case-insensitive):

```csharp
var csv = new[] { "Id,Name,ProfileId,ProfileName", "1,John,10,Admin", "2,Maria,20,Support" };
var list = _flow.FromText<CustomerCsvDto>(csv, TextDelimiter.Semicolon);
```

**Positional** (`hasHeader: false`) — matches by column order:

```csharp
var lines = new[] { "1,John", "2,Maria" };
var list = _flow.FromText<UserDto>(lines, TextDelimiter.Semicolon, hasHeader: false);
```

### Full-Text Search

```csharp
// Register FTS profile
public class ProductFtsProfile : FtsProfileDefinition
{
    public ProductFtsProfile()
    {
        Entity<Product>()
            .HasFullTextIndex(p => p.Name)
            .HasFullTextIndex(p => p.Description);
    }
}

// Search with automatic FTS condition injection
var results = await _flow.SearchFtsAsync<ProductDto>(
    "SELECT Id, Name, Description FROM Products WHERE Price > 100 ORDER BY Name",
    "keyword", new[] { "Name", "Description" });
// Injected SQL: SELECT Id, Name, Description FROM Products WHERE Price > 100
//               AND CONTAINS((Name, Description), @term)
//               ORDER BY Name
```

### Runtime Diagnostics

6 built-in rules fire automatically when `AddFlowMapper()` is registered:

```csharp
// Access diagnostics from the collector
var collector = sp.GetRequiredService<IDiagnosticCollector>();
var diagnostics = collector.Diagnostics;

// Or subscribe to telemetry events
var telemetry = sp.GetRequiredService<IDiagnosticTelemetry>();
telemetry.OnDiagnostic += d => Console.WriteLine($"{d.Code}: {d.Message}");

// Enable schema inspection (optional, default: false)
builder.ConfigureDiagnostics(opts => opts.EnableSchemaInspection = true);
```

| Code | Rule | Severity |
|------|------|----------|
| FM1001 | FTS index not verified/missing | Info / Warning |
| FM3002 | LIKE with leading wildcard | Warning |
| FM3003 | ORDER BY without index | Info |
| FM3005 | SELECT * detected | Info |
| FM3006 | Large OFFSET without WHERE | Warning |
| FM3007 | Cartesian JOIN (JOIN without ON) | Warning |

### Compile-time FTS Diagnostics (Source Generator)

```csharp
public class ProductFtsProfile : FtsProfileDefinition
{
    public ProductFtsProfile()
    {
        Entity<Product>()
            .HasFullTextIndex(p => p.Name);
            // ⚠ FM5001: Description is string but not configured with HasFullTextIndex
            // ⚠ FM5002: Price is decimal — not compatible with FTS (only string)
    }
}

public class Product
{
    public string Name { get; set; }
    public string Description { get; set; }  // FM5001
    public decimal Price { get; set; }       // FM5002
}
```

---

## Advanced Configuration

```csharp
services.AddFlowMapper(builder =>
{
    // Providers (with connection string)
    builder.AddProvider<SqlServerProvider>(connectionString);
    builder.AddProvider<PostgreSqlProvider>(connectionString);
    builder.AddProvider<MySqlProvider>(connectionString);
    builder.AddProvider<OracleProvider>(connectionString);

    // Provider (parameterless — reads connection from config)
    builder.AddProvider<SqlServerProvider>();

    // Profiles (mapping definitions)
    builder.AddProfile<AppProfile>();

    // FTS profiles (full-text search index definitions)
    builder.AddFtsProfile<CatalogFtsProfile>();

    // Data options
    builder.ConfigureData(opts =>
    {
        opts.CascadeSeparator = "_";
        opts.DefaultTimeout = 30;
        opts.FtsLanguage = "portuguese"; // PostgreSQL only
        opts.Retry.Enabled = true;
        opts.Retry.MaxRetries = 3;
        opts.Retry.InitialDelayMs = 100;
    });

    // Mapping options
    builder.ConfigureMapping(opts =>
    {
        opts.EnableFlatten = true;
        opts.PreferConstructorMapping = false;
        opts.EnableCache = true;
        opts.Strictness = StrictnessLevel.Warning;
    });
});

var flow = sp.GetRequiredService<IFlowMapper>();
```

---

## Project Structure

| Project | Description |
|---------|-------------|
| `FlowMapper.Abstractions` | Core interfaces (`IFlowMapper`, `IRapidMapper`, `IQueryExecutor`), enums, options |
| `FlowMapper.Core` | Profile definition, mapping expressions, `ForMember`/`ForPath` |
| `FlowMapper.Materializer` | `BuildPlanFlat<T>()`, `GroupBindings`, cascade materialization pipeline |
| `FlowMapper.Deserialization` | JSON/XML/TXT deserialization pipelines |
| `FlowMapper.Runtime` | `DataExecutionPipeline`, executors (`QueryExecutor`, `StreamExecutor`) |
| `FlowMapper.DependencyInjection` | `AddFlowMapper()`, `FlowMapperService`, DI wiring |
| `FlowMapper.Providers.*` | Database providers (SQL Server, PostgreSQL, MySQL, Oracle) |
| `FlowMapper.SourceGenerator` | Incremental source generator for compile-time mapper stubs |
| `FlowMapper.Compiler` | Compilation pipeline with 13 optimization passes |
| `FlowMapper.FullTextSearch` | FTS condition injection, `FtsSqlInjector`, `FullTextIndexRegistry` |
| `FlowMapper.FullTextSearch.Abstractions` | `IFullTextIndexRegistry`, `FtsIndexState`, `FtsProfileDefinition` |
| `FlowMapper.Mapping` | Object-object mapping pipeline with middlewares |
| `FlowMapper.Validation` | Rule-based validation pipeline |
| `FlowMapper.Diagnostics` | `DiagnosticEngine`, `SchemaInspector`, 6 rules, `IDiagnosticTelemetry` |
| `FlowMapper.SqlCompiler` | SQL compilation pipeline with dialect middlewares |
| `FlowMapper.PluginSdk` | Plugin system with 7 marker interfaces |
| `FlowMapper` | Umbrella meta-package |

---

## Ecosystem

```
        FlowCore (CQRS / Mediator)
              │
              ▼
         FlowMapper
         ┌──┴──┐
    Object    Data
    Mapping   Access
         │       │
         ▼       ▼
     FlowRuntime
         │
         ▼
   Applications
```

FlowMapper is part of a growing .NET ecosystem. The modular design allows each layer to be used independently.

---

## Roadmap

### Version 2.0
- ✅ Object-Object Mapping with fluent API
- ✅ Source Generator (compile-time `IMapper<,>`)
- ✅ Micro-ORM with nested materialization
- ✅ 4 SQL Providers (SQL Server, PostgreSQL, MySQL, Oracle)
- ✅ JSON, XML, TXT deserialization
- ✅ DI Integration (`AddFlowMapper`)

### Version 2.1
- ✅ Plugin SDK
- ✅ Compiler Pipeline with 13 optimization passes
- ✅ Validation Pipeline
- ✅ Execution Artifacts
- ✅ Full-Text Search (`SearchFtsAsync<T>`, 4 providers)
- ✅ Runtime Diagnostics (6 built-in rules + SchemaInspector)
- ✅ Diagnostic Telemetry (counters + OpenTelemetry event)
- ✅ Source Generator FTS warnings (FM5001/FM5002)

### Future
- 🔲 Query Optimizer
- 🔲 Additional Providers (SQLite, Cosmos DB)
- 🔲 Roslyn Analyzer Improvements
- 🔲 CLI scaffolding tool

---

## Documentation

| Language | Link |
|----------|------|
| 🇧🇷 Português | [`Docs/README.md`](Docs/README.md) |
| 🇺🇸 English | [`Docs/README.en.md`](Docs/README.en.md) |

---

## License

MIT
