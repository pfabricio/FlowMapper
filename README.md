# FlowMapper V2

All-in-one: compile-time object mapper (AutoMapper-like), micro-ORM, JSON/XML/TXT deserializer, and database providers — unified under a single `IFlowMapper` interface.

## Documentation

| Language | Link |
|----------|------|
| 🇧🇷 Português | [`Docs/README.md`](Docs/README.md) |
| 🇺🇸 English | [`Docs/README.en.md`](Docs/README.en.md) |

## Quick Start

```csharp
// DI
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=...;");
});

// Inject
public class MeuServico
{
    private readonly IFlowMapper _flow;
    public MeuServico(IFlowMapper flow) { _flow = flow; }

    public async Task Executar()
    {
        // SQL → DTO nested via alias
        var clientes = await _flow.QueryAsync<ClienteDto>(sql);

        // JSON → DTO nested
        var dto = _flow.FromJson<ClienteDto>(json);

        // JSON array → lista
        var lista = _flow.FromJsonList<Usuario>(jsonArray);

        // XML → DTO nested
        var dto = _flow.FromXml<ClienteDto>(xml);

        // TXT → DTO (flat: header ou posicional)
        var list = _flow.FromText<ClienteCsvDto>(csv, TextDelimiter.PontoVirgula);

        // Objeto → Objeto (AutoMapper-like)
        var dto = _flow.Map<Usuario, UsuarioDto>(usuario);
    }
}
```

## Features

### Object-Object Mapping (AutoMapper-like)

```csharp
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} ({s.Email})"))
            .ReverseMap();

        CreateMap<Cliente, ClienteDto>()
            .ForPath(d => d.Perfil.Nome, opt => opt.MapFrom(s => s.PerfilNome));
    }
}

var dto = _flow.Map<Usuario, UsuarioDto>(usuario);
```

### SQL → DTO with Cascade Nested Materialization

Auto-maps flat SQL resultsets into nested DTOs using column aliases and property-driven grouping.

```csharp
class ClienteDto {
    public int Id { get; set; }
    public string Nome { get; set; }
    public PerfilDto Perfil { get; set; }  // ← nested
}
class PerfilDto {
    public int Id { get; set; }
    public string Nome { get; set; }
}
```

```sql
SELECT u.Id, u.Nome,
       p.Id   AS Perfil_Id,   -- ← prefix "Perfil_"
       p.Nome AS Perfil_Nome  -- ← prefix "Perfil_"
FROM Usuario u
LEFT JOIN Perfil p ON p.UsuarioId = u.Id
```

```csharp
var clientes = await _flow.QueryAsync<ClienteDto>(sql);
// ClienteDto.Perfil is auto-populated for each row
```

### JSON → DTO (Nested)

```csharp
var json = """{ "Id": 1, "Nome": "Maria", "Perfil": { "Id": 10, "Nome": "Admin" } }""";
var dto = _flow.FromJson<ClienteDto>(json);
// dto.Perfil.Nome == "Admin"
```

### XML → DTO (Nested)

```csharp
var xml = """<ClienteDto><Id>1</Id><Nome>Pedro</Nome><Perfil><Id>10</Id><Nome>Suporte</Nome></Perfil></ClienteDto>""";
var dto = _flow.FromXml<ClienteDto>(xml);
```

### TXT → DTO (Flat)

**With header** — matches column names to property names (case-insensitive):

```csharp
var csv = new[] { "Id,Nome,PerfilId,PerfilNome", "1,Joao,10,Admin", "2,Maria,20,Suporte" };
var list = _flow.FromText<ClienteCsvDto>(csv, TextDelimiter.PontoVirgula);
```

**Positional** (`hasHeader: false`) — matches by column order:

```csharp
var lines = new[] { "1,Joao", "2,Maria" };
var list = _flow.FromText<Usuario>(lines, TextDelimiter.PontoVirgula, hasHeader: false);
```

## Complete Example

```csharp
// ── Config DI ──
var services = new ServiceCollection();
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=localhost;Database=FlowMapperDemo;...");
});
services.AddTransient<MeuServico>();
var sp = services.BuildServiceProvider();
await sp.GetRequiredService<MeuServico>().Executar();

// ── DTOs ──
public class Usuario       { public int Id { get; set; } public string Nome { get; set; } = ""; public string Email { get; set; } = ""; }
public class UsuarioDto    { public int Id { get; set; } public string NomeCompleto { get; set; } = ""; }
public class ClienteDto    { public int Id { get; set; } public string Nome { get; set; } = ""; public PerfilDto Perfil { get; set; } = null!; }
public class PerfilDto     { public int Id { get; set; } public string Nome { get; set; } = ""; }
public class ClienteCsvDto { public int Id { get; set; } public string Nome { get; set; } = ""; public int PerfilId { get; set; } public string PerfilNome { get; set; } = ""; }

// ── Profile ──
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} ({s.Email})"));
    }
}

// ── Service ──
public class MeuServico
{
    private readonly IFlowMapper _flow;
    public MeuServico(IFlowMapper flow) { _flow = flow; }

    public async Task Executar()
    {
        // 1. Map
        var usuario = new Usuario { Id = 1, Nome = "Joao", Email = "joao@email.com" };
        var dto = _flow.Map<Usuario, UsuarioDto>(usuario);
        Console.WriteLine($"Map: {dto.Id} - {dto.NomeCompleto}");

        // 2. SQL → nested
        var clientes = await _flow.QueryAsync<ClienteDto>(@"
            SELECT u.Id, u.Nome,
                   p.Id   AS Perfil_Id,
                   p.Nome AS Perfil_Nome
            FROM Usuario u
            LEFT JOIN Perfil p ON p.UsuarioId = u.Id");
        foreach (var c in clientes)
            Console.WriteLine($"SQL: {c.Id} / Perfil: {c.Perfil.Nome}");

        // 3. JSON → nested
        var json = """{ "Id": 1, "Nome": "Maria", "Perfil": { "Id": 10, "Nome": "Admin" } }""";
        var jDto = _flow.FromJson<ClienteDto>(json);
        Console.WriteLine($"JSON: {jDto.Perfil.Nome}");

        // 4. JSON array
        var jsonArray = """[{ "Id": 1, "Nome": "Joao" }, { "Id": 2, "Nome": "Maria" }]""";
        foreach (var u in _flow.FromJsonList<Usuario>(jsonArray))
            Console.WriteLine($"JSON[]: {u.Nome}");

        // 5. XML → nested
        var xml = """<ClienteDto><Id>1</Id><Nome>Pedro</Nome><Perfil><Id>10</Id><Nome>Suporte</Nome></Perfil></ClienteDto>""";
        var xDto = _flow.FromXml<ClienteDto>(xml);
        Console.WriteLine($"XML: {xDto.Perfil.Nome}");

        // 6. TXT com header
        var csv = new[] { "Id,Nome,PerfilId,PerfilNome", "1,Joao,10,Admin", "2,Maria,20,Suporte" };
        foreach (var c in _flow.FromText<ClienteCsvDto>(csv, TextDelimiter.PontoVirgula))
            Console.WriteLine($"TXT: {c.Nome} / PerfilId={c.PerfilId}");

        // 7. TXT posicional
        var pos = new[] { "1,Joao", "2,Maria" };
        foreach (var u in _flow.FromText<Usuario>(pos, TextDelimiter.PontoVirgula, hasHeader: false))
            Console.WriteLine($"TXT(pos): {u.Nome}");
    }
}
```

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
| `FlowMapper` | Umbrella meta-package |

## DI Registration

```csharp
services.AddFlowMapper(builder =>
{
    // Providers
    builder.AddProvider<SqlServerProvider>(connectionString);
    builder.AddProvider<PostgreSqlProvider>(connectionString);
    builder.AddProvider<MySqlProvider>(connectionString);
    builder.AddProvider<OracleProvider>(connectionString);

    // Profiles (mapping definitions)
    builder.AddProfile<AppProfile>();

    // Behaviors
    builder.AddBehavior<LoggingBehavior>();
    builder.AddBehavior<CachingBehavior>();

    // Options
    builder.ConfigureData(opts => opts.CascadeSeparator = "_");
    builder.ConfigureMapping(opts => opts.EnableFlatten = true);

    // Naming strategy
    builder.UseNamingStrategy<PascalCaseNamingStrategy>();

    // Retry
    builder.UseRetryStrategy(retry => retry.MaxRetries = 3);
});

var flow = sp.GetRequiredService<IFlowMapper>();
```

## License

MIT
