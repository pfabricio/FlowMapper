# FlowMapper V2

All-in-one: compile-time object mapper (AutoMapper-like), micro-ORM, JSON/XML/TXT deserializer, and database providers — unified under a single `IFlowMapper` interface.

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

- **Object-Object Mapping** — AutoMapper-like with `ForMember`/`ForPath`, profiles, `ReverseMap`
- **Micro-ORM** — `QueryAsync<T>`, `CommandAsync`, `StreamAsync<T>` with cascade nested materialization via column aliases
- **JSON Deserialization** — `FromJson<T>`, `FromJsonList<T>` with nested DTO support
- **XML Deserialization** — `FromXml<T>` with nested DTO support
- **TXT Deserialization** — `FromText<T>` flat-only, with header (case-insensitive) or positional mode
- **Database Providers** — SQL Server, PostgreSQL, MySQL, Oracle
- **Source Generator** — Compile-time mapper stubs (zero runtime reflection)
- **DI Integration** — `AddFlowMapper()` with profiles, providers, behaviors, options

## Packages

| Package | Description |
|---------|-------------|
| `FlowMapper` | Umbrella meta-package |
| `FlowMapper.Abstractions` | Core interfaces and enums |
| `FlowMapper.DependencyInjection` | DI wiring and `FlowMapperService` |

## License

MIT
