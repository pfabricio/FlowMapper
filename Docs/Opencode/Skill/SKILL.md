---
name: flowmapper
description: Use when working with FlowMapper — compile-time mapping, micro-ORM, deserialization, DI registration, profiles, pipelines, providers, source generator. Trigger on mentions of IFlowMapper, IRapidMapper, AddFlowMapper, ProfileDefinition, QueryAsync, MaterializationPlan, FlowMapperBuilder, IQueryExecutor, ForMember, ForPath, ReverseMap, CascadeSeparator.
---

# FlowMapper V2 Skill

Compile-time data mapping platform for .NET 8+. Combines object-object mapping, micro-ORM, JSON/XML/TXT deserialization, source generation, and execution pipelines.

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
  │    Mapping, SQL)      │
  └───────────┬───────────┘
              │
  ┌───────────┴───────────┐
  ▼                       ▼
Mapping Pipeline  Materialization Pipeline
(Object→Object)   (DataReader→Object)
  │                       │
  ▼                       ▼
Runtime Engine ───── Execution Scope
  │
  ▼
DTO / Entity
```

## Key Interfaces

### IFlowMapper (unified facade)
```csharp
// Object → Object
TDest Map<TSource, TDest>(TSource source);

// Micro-ORM
Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ...);
Task<T> QuerySingleAsync<T>(...);
Task<T?> QuerySingleOrDefaultAsync<T>(...);
Task<T> QueryScalarAsync<T>(...);
Task<int> ExecuteAsync(string sql, object? parameters = null, ...);
IAsyncEnumerable<T> StreamAsync<T>(string sql, ...);

// Deserialization
T FromJson<T>(string json);
IReadOnlyList<T> FromJsonList<T>(string json);
T FromXml<T>(string xml);
IReadOnlyList<T> FromText<T>(IEnumerable<string> lines, TextDelimiter delimiter, bool hasHeader = true);
```

### IRapidMapper (micro-ORM focused)
```csharp
Task<IEnumerable<T>> QueryAsync<T>(string sql, object? parameters = null, ...);
Task<T> QuerySingleAsync<T>(...);
Task<T> ExecuteScalarAsync<T>(...);
Task<int> ExecuteAsync(string sql, object? parameters = null, ...);
IAsyncEnumerable<T> StreamAsync<T>(string sql, ...);
```

### Providers
- `SqlServerProvider` (Microsoft.Data.SqlClient)
- `PostgreSqlProvider` (Npgsql)
- `MySqlProvider` (MySqlConnector)
- `OracleProvider` (Oracle.ManagedDataAccess)

### Delegate Types
```csharp
delegate TDestination MappingDelegate<TSource, TDestination>(TSource source);
delegate T MaterializationDelegate<T>(IDataReader reader);
delegate CompiledSql SqlDelegate(string sql, object? parameters);
```

## DI Registration

### Simple (defaults)
```csharp
services.AddFlowMapper();
```

### Advanced (with configuration)
```csharp
services.AddFlowMapper(builder =>
{
    builder.AddProvider<SqlServerProvider>(connectionString);
    builder.AddProfile<AppProfile>();
    builder.AddBehavior<LoggingBehavior>();
    builder.ConfigureData(opts => opts.CascadeSeparator = "_");
    builder.ConfigureMapping(opts => opts.EnableFlatten = true);
    builder.UseNamingStrategy<PascalCaseNamingStrategy>();
    builder.UseRetryStrategy(retry => retry.MaxRetries = 3);
});
```

### FlowMapperBuilder Methods
| Method | Purpose |
|--------|---------|
| `AddProvider<T>(string? connectionString)` | Register database provider |
| `AddProfile<T>()` | Register mapping profile |
| `AddBehavior<T>()` | Add pipeline behavior (middleware) |
| `ConfigureData(Action<DataOptions>)` | Data options (separator, timeout, retry) |
| `ConfigureMapping(Action<MappingOptions>)` | Mapping options (flatten, strictness, cache) |
| `UseNamingStrategy<T>()` | Set naming strategy |
| `UseRetryStrategy(Action<RetryOptions>)` | Configure retry |

## Profile / Mapping

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
```

### MappingExpression Methods
| Method | Description |
|--------|-------------|
| `ForMember(dest, opt => opt.MapFrom(src))` | Map specific member |
| `ForPath(dest nested, opt => opt.MapFrom(...))` | Map nested property path |
| `ReverseMap()` | Create reverse mapping |
| `ConstructUsing(expr)` | Custom construction |
| `IgnoreMember(name)` | Skip property |
| `AfterMap(action)` / `BeforeMap(action)` | Mapping lifecycle hooks |

## Micro-ORM / Nested Materialization

Flat SQL → nested DTO via column aliases with `CascadeSeparator` (default `_`):

```sql
SELECT u.Id, u.Name,
       p.Id   AS Profile_Id,    -- → CustomerDto.Profile.Id
       p.Name AS Profile_Name   -- → CustomerDto.Profile.Name
FROM Users u
LEFT JOIN Profiles p ON p.UserId = u.Id
```

```csharp
var customers = await flow.QueryAsync<CustomerDto>(sql);
```

### MaterializationPlan
```csharp
var plan = Materializer.BuildPlanFlat<T>();
// plan.Bindings — list of (ColumnName, PropertyName, PropertyType)
// plan.TargetType — typeof(T)
```

## Deserialization

| Method | Input | Output |
|--------|-------|--------|
| `FromJson<T>(json)` | JSON string | Single T |
| `FromJsonList<T>(json)` | JSON array string | List<T> |
| `FromXml<T>(xml)` | XML string | Single T |
| `FromText<T>(lines, delimiter, hasHeader)` | CSV lines | List<T> |

## NuGet Packages

Only `FlowMapper` meta-package is published. Sub-projects are internal.

## Project Structure

`samples/` — console apps demonstrating usage
`src/FlowMapper/` — meta-package (single NuGet entry point)
`src/FlowMapper.Core/` — ProfileDefinition, ForMember/ForPath
`src/FlowMapper.Abstractions/` — IFlowMapper, IRapidMapper, options
`src/FlowMapper.Runtime/` — DataExecutionPipeline
`src/FlowMapper.Materializer/` — BuildPlanFlat, materialization
`src/FlowMapper.Deserialization/` — JSON/XML/TXT
`src/FlowMapper.DependencyInjection/` — AddFlowMapper, builder
`src/FlowMapper.Providers.*/` — Database providers
`src/FlowMapper.Compiler/` — 13-pass compilation pipeline
`src/FlowMapper.Mapping/` — mapping pipeline with middlewares
`src/FlowMapper.Validation/` — rule-based validation
`src/FlowMapper.SqlCompiler/` — SQL dialect compilation
`src/FlowMapper.Diagnostics/` — event/middleware diagnostics
`src/FlowMapper.PluginSdk/` — plugin extension points
`src/FlowMapper.SourceGenerator/` — Roslyn incremental generator
`src/FlowMapper.Analyzers/` — Roslyn analyzers
`src/FlowMapper.BuildIntegration/` — MSBuild integration
`src/FlowMapper.Execution/Artifacts/` — IExecutionArtifact, ISqlArtifact etc.

## Pipelines (Middlewares)

Each pipeline supports middleware chains:

| Pipeline | Middleware Interface | Purpose |
|----------|-------------------|---------|
| Execution | `IPipelineBehavior` | Cross-cutting (logging, cache, retry) |
| Mapping | `IMappingMiddleware` | Object-object mapping |
| Materialization | `IMaterializationMiddleware` | DataReader → DTO |
| Validation | `IValidationMiddleware` | Rule validation |
| SQL Compiler | `ISqlMiddleware` | SQL compilation |
| Diagnostics | `IDiagnosticsMiddleware` | Telemetry/metrics |

## Conventions

- File-scoped namespaces (not block-scoped)
- `sealed record` for DTO/value types
- `I*` prefix for all interfaces
- `Async` suffix for async methods
- `CancellationToken ct` as last parameter (optional, default = default)
- Nullable reference types enabled
- Default cascade separator: `_`
- Default target: .NET 8.0
