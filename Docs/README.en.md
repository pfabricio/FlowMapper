# FlowMapper V2

**FlowMapper is a compile-time data mapping platform for .NET.**

It combines object-object mapping, micro-ORM, deserialization, source generation, and execution pipelines under a unified architecture focused on **performance**, **extensibility**, and **zero runtime reflection**.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Installation](#2-installation)
3. [Project Structure](#3-project-structure)
4. [Object-Object Mapping](#4-object-object-mapping)
   - [ProfileDefinition](#41-profiledefinition)
   - [MappingExpression](#42-mappingexpression)
   - [ForMember / ForPath](#43-formember--forpath)
   - [ReverseMap](#44-reversemap)
   - [ConstructUsing](#45-constructusing)
   - [Flatten Mapping](#46-flatten-mapping)
   - [Nested Mapping](#47-nested-mapping)
   - [Constructor Mapping](#48-constructor-mapping)
   - [AfterMap / BeforeMap](#49-aftermap--beforemap)
5. [Source Generator](#5-source-generator)
6. [Micro-ORM](#6-micro-orm)
   - [QueryAsync](#61-queryasync)
   - [QuerySingleAsync](#62-querysingleasync)
   - [StreamAsync](#63-streamasync)
   - [CommandAsync](#64-commandasync)
   - [Transactional Scope](#65-transactional-scope)
7. [Deserialization](#7-deserialization)
   - [JSON](#71-json)
   - [XML](#72-xml)
   - [TXT / CSV](#73-txt--csv)
8. [Database Providers](#8-database-providers)
   - [SQL Server](#81-sql-server)
   - [PostgreSQL](#82-postgresql)
   - [MySQL](#83-mysql)
   - [Oracle](#84-oracle)
9. [DI Integration](#9-di-integration)
10. [Pipelines](#10-pipelines)
    - [Execution Pipeline (IPipelineBehavior)](#101-execution-pipeline)
    - [Mapping Pipeline](#102-mapping-pipeline)
    - [Materialization Pipeline](#103-materialization-pipeline)
    - [Validation Pipeline](#104-validation-pipeline)
    - [Diagnostics Pipeline](#105-diagnostics-pipeline)
    - [SQL Compiler Pipeline](#106-sql-compiler-pipeline)
    - [Compiler Pipeline](#107-compiler-pipeline)
11. [Plugin SDK](#11-plugin-sdk)
12. [Caching](#12-caching)
13. [Primitives](#13-primitives)
14. [Execution Artifacts](#14-execution-artifacts)
15. [Complete Examples](#15-complete-examples)

---

## 1. Overview

FlowMapper V2 is an **all-in-one** .NET framework that unifies:

| Area | Description |
|------|-------------|
| **Object-Object Mapping** | Entity mapping (similar to AutoMapper) with profiles, fluent expressions, flattening, nesting, constructors |
| **Micro-ORM** | Async data access with automatic DTO materialization via column aliases |
| **Source Generator** | Compile-time code generation (`IMapper<,>`) with zero runtime reflection |
| **Deserialization** | JSON, XML and TXT/CSV to DTOs with nested support |
| **4 Providers** | SQL Server, PostgreSQL, MySQL, Oracle |
| **DI Integration** | `AddFlowMapper()` with fluent registration of profiles, providers and behaviors |

```csharp
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=...;");
});
```

### Architecture

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
   Mapping Pipeline     Materialization Pipeline
   (Object → Object)    (DataReader → Object)
         │                       │
         ▼                       ▼
   Runtime Engine ────────── Execution Scope
         │
         ▼
     DTO / Entity
```

### Why FlowMapper?

| Benefit | Description |
|---------|-------------|
| ✅ **Compile-time Mapping** | Source generator produces `IMapper<,>` code at build time |
| ✅ **Zero Runtime Reflection** | No `System.Reflection` in hot paths — faster startup & execution |
| ✅ **Source Generator** | Roslyn `IIncrementalGenerator` — errors show at compile time |
| ✅ **Native AOT Ready** | No dynamic code generation — works with `nativeaot` |
| ✅ **Nested Mapping** | Recursive object-to-object and SQL-to-DTO with aliases |
| ✅ **Flatten Mapping** | Auto-flatten `Address.Street` → `AddressStreet` with `_` separator |
| ✅ **Micro-ORM** | `QueryAsync<T>`, `StreamAsync<T>`, `CommandAsync<T>` with cascade materialization |
| ✅ **4 SQL Providers** | SQL Server, PostgreSQL, MySQL, Oracle — each with dialect-aware pagination |
| ✅ **Execution Pipelines** | Middleware-based `IPipelineBehavior` chain for cross-cutting concerns |
| ✅ **Materialization Pipeline** | Caching, conversion, and null-handling middlewares |
| ✅ **Validation Pipeline** | Rule-based validation with `IValidationRule` |
| ✅ **Full-Text Search** | `SearchFtsAsync<T>` with automatic FTS condition injection across all 4 providers |
| ✅ **Runtime Diagnostics** | 6 built-in rules (FTS index, LIKE wildcard, ORDER BY index, SELECT *, large OFFSET, cartesian JOIN) |
| ✅ **Diagnostics Pipeline** | Event and middleware-based diagnostics with metrics |
| ✅ **Schema Inspection** | Application-lifetime cache with `ISchemaInspector` for verifying FTS indexes in the database |
| ✅ **Diagnostic Telemetry** | Per-code counters and `OnDiagnostic` event for OpenTelemetry |
| ✅ **Compile-time FTS Analysis** | Source generator emits FM5001/FM5002 warnings for misconfigured FTS profiles |
| ✅ **Compiler Pipeline** | 13 optimization passes (flatten, fusion, constant eval, dead metadata elimination) |
| ✅ **Plugin SDK** | Extend everything: providers, stages, passes, rules, generators |
| ✅ **Deserialization** | JSON, XML, TXT/CSV — all with nested DTO support |
| ✅ **Caching** | 5 levels: external `ICacheProvider`, compiled delegates, flows, plans |

### Comparison

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

## 2. Installation

Add the NuGet package:

```xml
<PackageReference Include="FlowMapper" Version="2.1.0" />
```

**Prerequisites:** .NET 8.0+

---

## 3. Project Structure

```
src/
├── FlowMapper/                          # Meta-package (references all sub-projects)
├── FlowMapper.Abstractions/             # Public interfaces (IFlowMapper, IRapidMapper, etc.)
├── FlowMapper.Primitives/               # Value types (ArtifactId, ExecutionId, PipelineId, ProviderId)
├── FlowMapper.Core/                     # Object-object mapping engine
├── FlowMapper.Execution/                # Execution plan and artifacts model
├── FlowMapper.Mapping/                  # Mapping pipeline with middlewares
├── FlowMapper.Materializer/             # DataReader → object materialization pipeline
├── FlowMapper.Compiler/                 # Compilation pipeline with 13 optimization passes
├── FlowMapper.Runtime/                  # Runtime implementations (query, command, stream executors)
├── FlowMapper.Deserialization/          # JSON, XML, TXT deserialization
├── FlowMapper.FullTextSearch/           # Full-text search engine (SearchFtsAsync, FtsSqlInjector)
├── FlowMapper.FullTextSearch.Abstractions/ # FTS abstractions (IFullTextIndexRegistry, FtsIndexState)
├── FlowMapper.Diagnostics/              # Diagnostics pipeline with middlewares
├── FlowMapper.Validation/               # Rule-based validation pipeline
├── FlowMapper.SqlCompiler/              # SQL compilation pipeline with middlewares
├── FlowMapper.SourceGenerator/          # Roslyn code generator (compile-time)
├── FlowMapper.DependencyInjection/      # DI integration (AddFlowMapper)
├── FlowMapper.Providers.Abstractions/   # Database provider abstractions
├── FlowMapper.Providers.SqlServer/      # SQL Server provider
├── FlowMapper.Providers.PostgreSql/     # PostgreSQL provider
├── FlowMapper.Providers.MySql/          # MySQL provider
├── FlowMapper.Providers.Oracle/         # Oracle provider
├── FlowMapper.PluginSdk/                # Plugin SDK (IFlowMapperPlugin)
├── FlowMapper.Analyzers/                # Roslyn analyzers
├── FlowMapper.BuildIntegration/         # MSBuild integration
├── FlowMapper.SqlCompiler/              # SQL compiler
├── FlowMapper.Mapping/                  # Mapping pipeline
├── FlowMapper.Cli/                      # CLI tool
├── FlowMapper.Data/                     # Data utilities
├── FlowMapper.Csv/                      # CSV support
```

---

## 4. Object-Object Mapping

### 4.1 ProfileDefinition

Abstract base class for defining mapping profiles:

```csharp
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        ProfileName = "AppProfile";

        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.Name} ({s.Email})"));

        CreateMap<Order, OrderDto>()
            .ReverseMap()
            .DisableFlattenMapping();
    }
}
```

**Profile Properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ProfileName` | `string` | `"Default"` | Profile name |
| `Policy` | `MappingPolicy` | — | Global policy (strictness, flatten, constructor) |

**MappingPolicy:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Strictness` | `StrictnessLevel` | `Warning` | Behavior when unmapped properties are found |
| `EnableFlatten` | `bool` | `true` | Enable automatic flattening |
| `PreferConstructor` | `bool` | `false` | Prefer constructor mapping |

**StrictnessLevel:**

| Value | Description |
|-------|-------------|
| `None` | Ignore unmapped properties |
| `Warning` | Warning only |
| `Error` | Throw error |

### 4.2 MappingExpression

Returned by `CreateMap<T1, T2>()`, provides the fluent API:

```csharp
CreateMap<Source, Dest>()
    .ForMember(d => d.Property, opt => opt.MapFrom(s => s.Other))
    .ForPath(d => d.Address.Street, opt => opt.MapFrom(s => s.Address.StreetName))
    .ReverseMap()
    .ConstructUsing(s => new Dest(s.Id, s.Name))
    .DisableFlattenMapping();
```

### 4.3 ForMember / ForPath

```csharp
// ForMember — simple property mapping
CreateMap<User, UserDto>()
    .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
    .ForMember(d => d.Password, opt => opt.Ignore());

// ForPath — nested property mapping
CreateMap<Order, OrderDto>()
    .ForPath(d => d.CustomerName, opt => opt.MapFrom(s => s.Customer.Name));
```

**MemberOptions:**

| Method | Description |
|--------|-------------|
| `MapFrom(string sourceProperty)` | Map from a property by name |
| `MapFrom(Expression<Func<TSource, object?>>)` | Map from an expression |
| `Ignore()` | Ignore the property |

### 4.4 ReverseMap

Automatically creates the reverse mapping:

```csharp
CreateMap<User, UserDto>()
    .ReverseMap();
// Equivalent to CreateMap<UserDto, User>() with the same inverted settings
```

### 4.5 ConstructUsing

Defines a custom factory for creating the destination object:

```csharp
// Factory method
CreateMap<Input, Output>()
    .ConstructUsing(CreateOutput);

static Output CreateOutput(Input source) => new(source.Value * 2);

// Inline lambda
CreateMap<Input, Output>()
    .ConstructUsing(source => new Output(source.Value * 2));
```

### 4.6 Flatten Mapping

**Enabled by default.** Transforms nested properties into "flattened" properties using `_` separator:

```csharp
public class Employee
{
    public string Name { get; set; }
    public Address Address { get; set; }    // → EmployeeDto.AddressStreet
}                                           // → EmployeeDto.AddressCity

public class EmployeeDto
{
    public string Name { get; set; }
    public string AddressStreet { get; set; }
    public string AddressCity { get; set; }
}
```

To disable per-mapping:

```csharp
CreateMap<Employee, EmployeeDto>()
    .DisableFlattenMapping();
```

### 4.7 Nested Mapping

Automatically detected for complex types (class, not-string, not-value-type, not-IEnumerable):

```csharp
public class Order
{
    public Customer Customer { get; set; }
}

public class OrderDto
{
    public CustomerDto Customer { get; set; }
}
// Order.Customer.Name → OrderDto.Customer.Name (recursive)
```

### 4.8 Constructor Mapping

Support for immutable types (records) and `ConstructUsing`:

```csharp
// Record with primary constructor
public record PersonDto(int Id, string Name);

// The Source Generator automatically detects constructor parameters
```

### 4.9 AfterMap / BeforeMap

Callbacks executed after/before mapping:

```csharp
// Method group
CreateMap<Order, OrderDto>()
    .AfterMap(CalculateTotals);

static void CalculateTotals(Order source, OrderDto target)
{
    target.Total = source.Price * source.Quantity;
}

// Inline lambda
CreateMap<Order, OrderDto>()
    .AfterMap((source, target) => target.Total = source.Price * source.Quantity);
```

---

## 5. Source Generator

**FlowMapperGenerator** is a Roslyn `IIncrementalGenerator` that generates `IMapper<,>` implementations at **compile-time**, eliminating runtime reflection.

**Source Generator Benefits:**

| Benefit | Description |
|---------|-------------|
| ⚡ **No runtime reflection** | All mapping is compiled C# — no `System.Reflection` in hot paths |
| 🚀 **Faster startup** | No runtime expression compilation; code is ready at startup |
| 🔍 **Better debugging** | Step through generated code like any other class |
| ✅ **Compile-time validation** | Mapping errors appear during build, not in production |
| 📦 **Reduced allocations** | Optimized code without dynamic delegates or reflection dictionaries |
| 🏗️ **Native AOT friendly** | No dynamic IL generation — compatible with `nativeaot` publish |

**How it works:**

1. Detects classes that inherit from `ProfileDefinition`
2. Scans `CreateMap<>()` calls inside constructors
3. Extracts explicit mappings, `ForMember`, `ForPath`, `AfterMap`, `ConstructUsing`
4. Generates C# code with complete `IMapper<,>` implementations

**Output:** `FlowMapper_Mappers.g.cs` with compiled mappers.

**Generated code example:**

```csharp
// Auto-generated
public partial class UserToUserDtoMapper : IMapper<User, UserDto>
{
    public UserDto Map(User source)
    {
        if (source is null) throw new ArgumentNullException(nameof(source));
        var target = new UserDto();
        target.Id = source.Id;
        target.Name = source.Name;
        target.Address = MapAddress(source.Address);
        return target;
    }

    private AddressDto MapAddress(Address source) { ... }
}
```

The Source Generator also supports the `[Map<T1, T2>]` attribute (legacy):

```csharp
[Map<User, UserDto>]
public partial class UserMapper : IMapper<User, UserDto>;
```

---

## 6. Micro-ORM

### 6.1 QueryAsync

Returns multiple rows mapped to DTOs:

```csharp
var users = await flow.QueryAsync<UserDto>(
    "SELECT Id, Name, Email FROM Users WHERE Active = @Active",
    new { Active = true });
```

**Nested DTOs via aliases:**

```csharp
var data = await flow.QueryAsync<CustomerDto>(@"
    SELECT
        u.Id,
        u.Name,
        p.Id   AS Profile_Id,
        p.Name AS Profile_Name
    FROM Users u
    LEFT JOIN Profile p ON p.UserId = u.Id");
// → CustomerDto.Profile.Id and CustomerDto.Profile.Name populated
```

### 6.2 QuerySingleAsync

Returns exactly one row (throws if not found):

```csharp
var user = await flow.QuerySingleAsync<UserDto>(
    "SELECT Id, Name FROM Users WHERE Id = @Id",
    new { Id = 1 });
```

### 6.3 StreamAsync

Async streaming of results (does not load everything into memory):

```csharp
await foreach (var user in flow.StreamAsync<UserDto>(
    "SELECT Id, Name FROM Users"))
{
    Process(user);
}
```

### 6.4 CommandAsync

Executes commands (INSERT, UPDATE, DELETE) and returns affected rows:

```csharp
var affected = await flow.CommandAsync(
    "UPDATE Users SET Name = @Name WHERE Id = @Id",
    new { Id = 1, Name = "John" });

var newId = await flow.CommandScalarAsync<int>(
    "INSERT INTO Users (Name) VALUES (@Name); SELECT SCOPE_IDENTITY()",
    new { Name = "Maria" });
```

### 6.5 Transactional Scope

```csharp
await using var scope = flow.CreateScope(transactional: true);

await flow.CommandAsync("INSERT INTO Order ...", new { ... });
await flow.CommandAsync("UPDATE Stock ...", new { ... });

await scope.CommitAsync();
// If CommitAsync() is not called, it Rollbacks on DisposeAsync()
```

---

## 7. Deserialization

### 7.1 JSON

Support for objects and lists with nested DTOs:

```csharp
// Single object
var dto = flow.FromJson<CustomerDto>(@"{
    ""Id"": 1,
    ""Name"": ""Maria"",
    ""Profile"": { ""Id"": 10, ""Name"": ""Admin"" }
}");

// List
var list = flow.FromJsonList<UserDto>(jsonArray);
```

### 7.2 XML

```csharp
var dto = flow.FromXml<CustomerDto>(@"
    <CustomerDto>
        <Id>1</Id>
        <Name>Peter</Name>
        <Profile>
            <Id>10</Id>
            <Name>Admin</Name>
        </Profile>
    </CustomerDto>");
```

### 7.3 TXT / CSV

Support for `;` (Semicolon) and `\t` (Tab) delimiters, with or without header:

```csharp
// CSV with header (case-insensitive)
var list = flow.FromText<CustomerCsvDto>(csvLines,
    TextDelimiter.Semicolon,
    hasHeader: true);

// Positional CSV (no header — maps by column order)
var list = flow.FromText<UserDto>(lines,
    TextDelimiter.Semicolon,
    hasHeader: false);
```

---

## 8. Database Providers

### 8.1 SQL Server

| Property | Value |
|----------|-------|
| Class | `SqlServerProvider` |
| Connection | `SqlConnection` |
| Pagination | `OFFSET x ROWS FETCH NEXT y ROWS ONLY` |
| Identity | `SELECT SCOPE_IDENTITY()` |
| Parameters | `@name` |

```csharp
builder.AddProvider<SqlServerProvider>("Server=localhost;Database=MyDb;Trusted_Connection=True;");
```

### 8.2 PostgreSQL

| Property | Value |
|----------|-------|
| Class | `PostgreSqlProvider` |
| Connection | `NpgsqlConnection` |
| Pagination | `LIMIT y OFFSET x` |
| Identity | `SELECT LASTVAL()` |
| Parameters | `@name` |

```csharp
builder.AddProvider<PostgreSqlProvider>("Host=localhost;Database=MyDb;Username=user;Password=pass;");
```

### 8.3 MySQL

| Property | Value |
|----------|-------|
| Class | `MySqlProvider` |
| Connection | `MySqlConnection` |
| Pagination | `LIMIT y OFFSET x` |
| Identity | `SELECT LAST_INSERT_ID()` |
| Parameters | `@name` |

```csharp
builder.AddProvider<MySqlProvider>("Server=localhost;Database=MyDb;Uid=root;Pwd=pass;");
```

### 8.4 Oracle

| Property | Value |
|----------|-------|
| Class | `OracleProvider` |
| Connection | `OracleConnection` |
| Pagination | ROWNUM subquery |
| Identity | `SELECT LAST_INSERT_ID()` |
| Parameters | `:name` |

```csharp
builder.AddProvider<OracleProvider>("Data Source=localhost:1521/MyDb;User Id=user;Password=pass;");
```

---

## 9. DI Integration

Registers all services with default configuration:

```csharp
services.AddFlowMapper();
```

### Advanced Configuration

To customize providers, profiles and options:

```csharp
services.AddFlowMapper(builder =>
{
    // Mapping profile
    builder.AddProfile<AppProfile>();

    // Database provider
    builder.AddProvider<SqlServerProvider>("Server=...;");

    // Global configuration
    builder.ConfigureData(opts =>
    {
        opts.DefaultTimeout = 30;
        opts.CascadeSeparator = "_";
        opts.Retry.Enabled = true;
        opts.Retry.MaxRetries = 3;
        opts.Retry.InitialDelayMs = 100;
    });
    builder.ConfigureMapping(opts =>
    {
        opts.DefaultProfile = "AppProfile";
        opts.EnableFlatten = true;
        opts.Strictness = StrictnessLevel.Warning;
    });
});
```

**Auto-registered services:**

| Service | Implementation | Lifetime |
|---------|---------------|----------|
| `IFlowMapper` | `FlowMapperService` | Singleton |
| `IRapidMapper` | `RapidMapperService` | Singleton |
| `IMaterializer` | `Materializer` | Singleton |
| `IDeserializer` | `DeserializationPipeline` | Singleton |
| `ICompiler` | `Compiler` | Singleton |
| `IConnectionFactory` | `ConnectionFactory` | Singleton |
| `IPipelineExecutor` | `PipelineExecutor` | Singleton |
| `IQueryExecutor` | `QueryExecutor` | Singleton |
| `ICommandExecutor` | `CommandExecutor` | Singleton |
| `IStreamExecutor` | `StreamExecutor` | Singleton |
| `IExecutionScopeFactory` | `ExecutionScopeFactory` | Singleton |
| `FlowBuilder` | `FlowBuilder` | Singleton |

---

## 10. Pipelines

### 10.1 Execution Pipeline

HTTP-middleware-style execution pipeline with `IPipelineBehavior`:

```csharp
public class LoggingBehavior : IPipelineBehavior, IOrderedBehavior
{
    public int Order => 0;

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;

    public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
    {
        Console.WriteLine($"[{context.Phase}] {context.Sql}");
        await next();
    }
}

// Registration
services.AddFlowMapper();
services.AddTransient<IPipelineBehavior, LoggingBehavior>();
```

**ExecutionPhase:**

| Phase | Description |
|-------|-------------|
| `BeforeExecute` | Before execution |
| `Execute` | Command/query execution |
| `Mapping` | Result mapping |
| `RowRead` | Individual row reading |
| `AfterExecute` | After execution |
| `Completed` | Finished |

### 10.2 Mapping Pipeline

Internal pipeline for object-object mapping:

| Middleware | Description |
|------------|-------------|
| `NullPropagationMiddleware` | Throws `ArgumentNullException` if source is null |
| `FlattenMiddleware` | Flatten mapping support |

### 10.3 Materialization Pipeline

Pipeline for `DataReader` → object materialization:

| Middleware | Description |
|------------|-------------|
| `CachingMiddleware` | Compiled delegate caching |
| `ConversionMiddleware` | Type conversion |
| `NullValueHandlingMiddleware` | DBNull handling |

### 10.4 Validation Pipeline

Rule-based validation pipeline:

```csharp
public class NotNullRule : ValidationRule
{
    public override string Name => "NotNull";

    public override ValidationResult Validate<T>(T target, IExecutionArtifact? artifact)
    {
        if (target is null)
            return Fail("Target cannot be null");
        return Success();
    }
}
```

### 10.5 Diagnostics Pipeline

Event and middleware-based diagnostics pipeline:

```csharp
public class MetricsMiddleware : IDiagnosticsMiddleware
{
    public void Process(DiagnosticEvent @event, DiagnosticsDelegate next)
    {
        var sw = Stopwatch.StartNew();
        next(@event);
        sw.Stop();
        Console.WriteLine($"{@event.Category}: {sw.ElapsedMilliseconds}ms");
    }
}
```

### 10.6 SQL Compiler Pipeline

SQL compilation pipeline:

| Middleware | Description |
|------------|-------------|
| `DialectMiddleware` | Applies dialect (pagination, parameters) |
| `ParameterNormalizationMiddleware` | Normalizes parameter names |
| `SqlCachingMiddleware` | Compiled SQL caching |

### 10.7 Compiler Pipeline

Multi-stage compilation pipeline:

1. **MetadataStage** — Builds type metadata
2. **OptimizationStage** — Applies 13 optimization passes
3. **ValidationStage** — Validates artifacts
4. **ExecutionPlanStage** — Builds execution plans
5. **SourceGenerationStage** — Generates source code

**Optimization Passes:**

| Pass | Description |
|------|-------------|
| `FlattenOptimizationPass` | Optimizes flatten mapping |
| `NestedOptimizationPass` | Optimizes nested mapping |
| `DelegateFusionPass` | Fuses multiple delegates into one |
| `ConstantEvaluationPass` | Evaluates constant expressions |
| `NullOptimizationPass` | Optimizes null checks |
| `ConverterOptimizationPass` | Optimizes type converters |
| `RedundantMappingRemovalPass` | Removes redundant mappings |
| `DeadMetadataEliminationPass` | Eliminates unused metadata |

---

## 11. Plugin SDK

Complete plugin system with 7 categories:

```csharp
public class MyPlugin : IFlowMapperPlugin, ICompilerPlugin
{
    public string Name => "MyPlugin";
    public Version Version => new(1, 0);

    public void Configure(IPluginBuilder builder)
    {
        builder.AddOptimizationPass(typeof(MyOptimizationPass));
        builder.AddCompilerStage(typeof(MyStage));
    }

    public IReadOnlyCollection<Type> GetStageTypes() => new[] { typeof(MyStage) };
}
```

**Marker Interfaces:**

| Interface | Category |
|-----------|----------|
| `ICompilerPlugin` | Adds stages to the compiler pipeline |
| `IProviderPlugin` | Database provider |
| `IRuntimePlugin` | Runtime services |
| `IValidationPlugin` | Validation rules |
| `IOptimizationPlugin` | Optimization passes |
| `IDiagnosticsPlugin` | Diagnostics middlewares |
| `ISourceGeneratorPlugin` | Source code generators |

---

## 12. Caching

Multiple caching levels:

| Level | Location | Description |
|-------|----------|-------------|
| `ICacheProvider` | Abstraction | Interface for external cache (Redis, MemoryCache, etc.) |
| `CachingMiddleware` | Materialization | Compiled delegate cache (`MaterializationDelegate<T>`) |
| `FlowCache` | Source Generator | Built flow cache |
| `FlowBuilder` | Core | `Flow` object cache in `ConcurrentDictionary<(Type, Type), Flow>` |
| `ExecutionOptions` | Runtime | Optional per-key cache via `CacheKey`/`CacheExpiration` |

```csharp
// Per-execution cache
var result = await flow.QueryAsync<UserDto>(sql,
    new { Active = true },
    new ExecutionOptions { CacheKey = "active-users", CacheExpiration = TimeSpan.FromMinutes(5) });

// Custom cache provider
public class RedisCacheProvider : ICacheProvider
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) { ... }
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) { ... }
    public Task RemoveAsync(string key, CancellationToken ct = default) { ... }
}
```

---

## 13. Primitives

Strongly-typed value types:

```csharp
public readonly record struct ArtifactId(Guid Value)
{
    public static ArtifactId New() => new(Guid.NewGuid());
}

public readonly record struct ExecutionId(Guid Value) { public static ExecutionId New() => new(Guid.NewGuid()); }
public readonly record struct PipelineId(Guid Value) { public static PipelineId New() => new(Guid.NewGuid()); }
public readonly record struct ProviderId(string Name);
```

---

## 14. Execution Artifacts

Execution artifacts produced by the Compiler Pipeline:

| Artifact | Description |
|----------|-------------|
| `IMappingArtifact` | Contains `MappingDelegate`, `ReverseMappingDelegate`, `BeforeMapDelegate`, `AfterMapDelegate` |
| `IMaterializationArtifact` | Contains `MaterializationDelegate`, `ColumnBindings`, `ConstructorDelegate` |
| `ISqlArtifact` | Contains `CommandText`, `Parameters`, `ExecutionDelegate` |
| `IProviderArtifact` | Contains `ProviderName`, `ProviderVersion` |
| `IMetadataArtifact` | Contains `ITypeInfo` collection |
| `IConstructorArtifact` | Contains `FactoryDelegate`, `ConstructorParameterBindings` |
| `IExecutionPlan` | Groups multiple artifacts with an `ExecutionDelegate` |

---

## 15. Complete Examples

### Example 1: Basic Mapping

```csharp
// 1. Define the profile
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.FullName, opt => opt.MapFrom(s => $"{s.Name} ({s.Email})"));
    }
}

public class User { public int Id { get; set; } public string Name { get; set; } public string Email { get; set; } }
public class UserDto { public int Id { get; set; } public string FullName { get; set; } }

// 2. Configure DI
services.AddFlowMapper(builder => builder.AddProfile<AppProfile>());
var flow = services.BuildServiceProvider().GetRequiredService<IFlowMapper>();

// 3. Map
var dto = flow.Map<User, UserDto>(new User { Id = 1, Name = "John", Email = "john@email.com" });
```

### Example 2: Complete Micro-ORM

```csharp
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=localhost;Database=Store;Trusted_Connection=True;");
});

var flow = serviceProvider.GetRequiredService<IFlowMapper>();

// Query with nested DTO
var orders = await flow.QueryAsync<OrderDto>(@"
    SELECT
        o.Id,
        o.Total,
        c.Id   AS Customer_Id,
        c.Name AS Customer_Name
    FROM Orders o
    JOIN Customer c ON c.Id = o.CustomerId
    WHERE o.Total > @Minimum",
    new { Minimum = 100.0m });

// Insert with ID return
var newId = await flow.CommandScalarAsync<int>(
    "INSERT INTO Product (Name, Price) VALUES (@Name, @Price); SELECT SCOPE_IDENTITY();",
    new { Name = "Keyboard", Price = 199.90m });

// Transaction
await using var scope = flow.CreateScope(transactional: true);
await flow.CommandAsync("INSERT INTO Orders (CustomerId, Total) VALUES (@CustomerId, @Total)", new { CustomerId = 1, Total = 50.0m });
await flow.CommandAsync("UPDATE Product SET Stock = Stock - 1 WHERE Id = @Id", new { Id = 5 });
await scope.CommitAsync();
```

### Example 3: Multi-format Deserialization

```csharp
var flow = serviceProvider.GetRequiredService<IFlowMapper>();

// JSON with nesting
var json = """{"Id":1,"Name":"Maria","Profile":{"Id":10,"Name":"Admin"}}""";
var dto = flow.FromJson<CustomerDto>(json);

// XML
var xml = """<CustomerDto><Id>1</Id><Name>Peter</Name><Profile><Id>10</Id><Name>Admin</Name></Profile></CustomerDto>""";
var dto2 = flow.FromXml<CustomerDto>(xml);

// CSV
var csv = new[] { "Id;Name;Email", "1;John;john@email.com", "2;Maria;maria@email.com" };
var list = flow.FromText<UserDto>(csv, TextDelimiter.Semicolon, hasHeader: true);
```

### Example 4: Custom Pipeline Behavior

```csharp
public class LoggingBehavior : IPipelineBehavior, IOrderedBehavior
{
    public int Order => 0;

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;

    public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Type={typeof(T).Name} SQL={context.Sql}");
        var sw = Stopwatch.StartNew();
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERROR] {ex.Message}");
            throw;
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[{sw.ElapsedMilliseconds}ms] Completed");
        }
    }
}

// Registration
services.AddFlowMapper();
services.AddSingleton<IPipelineBehavior, LoggingBehavior>();
```

### Example 5: Plugin

```csharp
public class MyProviderPlugin : IFlowMapperPlugin, IProviderPlugin
{
    public string Name => "MyDB";
    public Version Version => new(1, 0);

    public void Configure(IPluginBuilder builder)
    {
        builder.AddProvider<MyProvider>();
    }
}

// Load plugins
var loader = new PluginLoader();
loader.LoadFromAssembly(typeof(MyProviderPlugin).Assembly);
```

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
- ✅ Full-Text Search (`SearchFtsAsync<T>` across all 4 providers)
- ✅ Runtime Diagnostics (6 built-in rules)
- ✅ Diagnostics Pipeline
- ✅ Validation Pipeline
- ✅ Execution Artifacts
- ✅ Schema Inspection (ISchemaInspector with cache)
- ✅ Diagnostic Telemetry (counters + OnDiagnostic event)
- ✅ Compile-time FTS Analysis (FM5001/FM5002)

### Future
- 🔲 Query Optimizer
- 🔲 Additional Providers (SQLite, Cosmos DB)
- 🔲 Roslyn Analyzer Improvements
- 🔲 CLI scaffolding tool

---

## License

MIT
