---
name: rapid-mapper
description: Use when working with RapidMapper micro-ORM — setup, configuration, queries, commands, streaming, pipeline behaviors, cache, retry, telemetry, provider selection, or mapping. Trigger on mentions of RapidMapper, IRapidMapper, QueryAsync, CommandExecutor, StreamAsync, PipelineBehavior, ICacheProvider, GenericProvider, or micro-ORM patterns.
---

# RapidMapper Skill

Micro-ORM para .NET 8+ com pipeline extensível, SQL explícito e controle total de execução.

## Quando Usar

- Executar SQL direto com mapeamento automático para objetos
- Streaming de grandes result sets com `IAsyncEnumerable`
- Transações com auto commit/rollback
- Interceptar execuções via pipeline behaviors (logging, retry, etc.)
- Multi-database (SQL Server, PostgreSQL, MySQL, Oracle, SQLite, etc.)
- Cache de queries com ICacheProvider
- Retry com backoff exponencial
- OpenTelemetry integrado

## Setup

### 1. Referenciar os projetos

```xml
<ItemGroup>
  <ProjectReference Include="..\RapidMapper\RapidMapper.Execution\RapidMapper.Execution.csproj" />
  <ProjectReference Include="..\RapidMapper\RapidMapper.DependencyInjection\RapidMapper.DependencyInjection.csproj" />
  <!-- Escolha um ou ambos os providers -->
  <ProjectReference Include="..\RapidMapper\RapidMapper.Provider.SqlServer\RapidMapper.Provider.SqlServer.csproj" />
  <ProjectReference Include="..\RapidMapper\RapidMapper.Provider.PostgreSql\RapidMapper.Provider.PostgreSql.csproj" />
</ItemGroup>
```

### 2. Configurar no DI (`Program.cs`)

```csharp
using RapidMapper.DependencyInjection;
using RapidMapper.Provider.SqlServer; // ou PostgreSql

builder.Services.AddRapidMapper(builder =>
{
    // Provider
    builder.AddProvider<SqlServerProvider>();

    // Retry com backoff exponencial
    builder.UseRetryStrategy(maxRetries: 3, initialDelayMs: 200);

    // Cache de queries
    builder.UseCacheProvider<MemoryCacheProvider>();

    // Dialeto SQL
    builder.AddDialect<SqlServerDialect>();

    // Naming strategy (snake_case → PascalCase)
    builder.UseNamingStrategy<DefaultNamingStrategy>();

    // Behavior customizado
    builder.AddBehavior<LoggingBehavior>();

    // Opções globais
    builder.Configure(options =>
    {
        options.DefaultTimeout = 30;
        options.Mapping.Separator = "_";
    });
});
```

### 3. Injetar e usar

```csharp
public class UserRepository
{
    private readonly IRapidMapper _mapper;

    public UserRepository(IRapidMapper mapper)
    {
        _mapper = mapper;
    }
}
```

## API Principal (`IRapidMapper`)

### Queries

```csharp
// Múltiplos resultados
var users = await _mapper.QueryAsync<User>(
    "SELECT Id, Nome, Email FROM Usuarios WHERE Ativo = 1"
);

// Resultado único (lança se != 1)
var user = await _mapper.QuerySingleAsync<User>(
    "SELECT Id, Nome FROM Usuarios WHERE Id = @Id",
    new { Id = 1 }
);

// Único ou null (lança se > 1)
var user = await _mapper.QuerySingleOrDefaultAsync<User>(
    "SELECT Id, Nome FROM Usuarios WHERE Email = @Email",
    new { Email = "teste@email.com" }
);

// Escalar
var count = await _mapper.QueryScalarAsync<int>(
    "SELECT COUNT(*) FROM Usuarios"
);
```

### Commands

```csharp
// INSERT, UPDATE, DELETE — retorna rows afetadas
var affected = await _mapper.ExecuteAsync(
    "UPDATE Usuarios SET Nome = @Nome WHERE Id = @Id",
    new { Id = 1, Nome = "Novo Nome" }
);

// Escalar em command
var newId = await _mapper.ExecuteScalarAsync<long>(
    "INSERT INTO Usuarios (Nome) VALUES (@Nome); SELECT SCOPE_IDENTITY();",
    new { Nome = "João" }
);
```

### Streaming

```csharp
// Lazy real — não materializa lista em memória
await foreach (var user in _mapper.StreamAsync<User>(
    "SELECT Id, Nome, Email FROM Usuarios ORDER BY Nome"
))
{
    Process(user);
}
```

### Transações

```csharp
// Via escopo manual
await using var scope = _mapper.CreateScope(transactional: true);

await _mapper.ExecuteAsync(
    "INSERT INTO Pedidos (UsuarioId, Total) VALUES (@UsuarioId, @Total)",
    new { UsuarioId = 1, Total = 99.90 }
);

await _mapper.ExecuteAsync(
    "UPDATE Estoque SET Quantidade = Quantidade - 1 WHERE ProdutoId = @ProdutoId",
    new { ProdutoId = 5 }
);

// Commit automático no fim do escopo se não houver exceção
// Rollback automático se houver exceção ou se não der commit
```

## Mapping

### Regras de mapeamento

- **Expression Trees compilados**: primeira execução gera delegate, subsequentes usam direto
- **Case-insensitive**: `nome` → `Nome`, `EMAIL` → `Email`
- **Nested mapping** com separador `_`: coluna `Perfil_Id` → propriedade `Perfil.Id`
- **Naming strategy automática**: `user_name` → `UserName` (com `DefaultNamingStrategy`)
- **Enums**: convertidos automaticamente
- **Nullable types**: suportados
- **DBNull**: convertido para `null`

### Exemplo de nested mapping

```csharp
public class Usuario
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
    public Perfil Perfil { get; set; } = new();
}

public class Perfil
{
    public int Id { get; set; }
    public string Nome { get; set; } = "";
}
```

SQL:
```sql
SELECT 
    u.Id, 
    u.Nome, 
    p.Id AS Perfil_Id, 
    p.Nome AS Perfil_Nome
FROM Usuarios u
JOIN Perfis p ON u.PerfilId = p.Id
```

## Pipeline Behaviors

### Interface

```csharp
using RapidMapper.Abstractions;

public class LoggingBehavior : IPipelineBehavior
{
    public bool ShouldExecute<T>(ExecutionContext<T> context)
    {
        // Filtrar quando executar (ex: só queries)
        return true;
    }

    public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
    {
        Console.WriteLine($"[SQL] {context.Sql}");
        
        await next();
        
        Console.WriteLine($"[Fase] {context.Phase} | [Rows] {context.Metrics.RowCount}");
    }
}
```

### Ordenação explícita

```csharp
public class CacheBehavior : IPipelineBehavior, IOrderedBehavior
{
    public int Order => 50; // Executa antes de behaviors com Order 1000

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;

    public Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next) => next();
}
```

### Registrar behavior

```csharp
builder.Services.AddRapidMapper(builder =>
{
    builder.AddProvider<SqlServerProvider>();
    builder.AddBehavior<LoggingBehavior>();
    builder.AddBehavior<CacheBehavior>();
});
```

### Fases do Pipeline

| Fase | Quando |
|------|--------|
| `BeforeExecute` | Antes da execução SQL |
| `Execute` | Durante execução do provider |
| `Mapping` | Após execução, antes do mapping |
| `RowRead` | Cada linha lida (streaming) |
| `AfterExecute` | Após execução completa |
| `Completed` | Commit da transação |

## Query Cache

### Implementar ICacheProvider

```csharp
using Microsoft.Extensions.Caching.Memory;
using RapidMapper.Abstractions;

public class MemoryCacheProvider : ICacheProvider
{
    private readonly IMemoryCache _cache;

    public MemoryCacheProvider(IMemoryCache cache) => _cache = cache;

    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue) options.AbsoluteExpirationRelativeToNow = expiration.Value;
        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken ct = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
```

### Usar cache

```csharp
var users = await _mapper.QueryAsync<User>(
    "SELECT * FROM Usuarios WHERE Categoria = @Cat",
    new { Cat = "Admin" },
    new ExecutionOptions
    {
        CacheKey = "users:admin",
        CacheExpiration = TimeSpan.FromMinutes(5)
    }
);
```

## Retry

```csharp
builder.UseRetryStrategy(maxRetries: 3, initialDelayMs: 200);
```

Backoff exponencial: 200ms → 400ms → 800ms

## ExecutionOptions (per-operation)

```csharp
var users = await _mapper.QueryAsync<User>(
    "SELECT * FROM Usuarios",
    options: new ExecutionOptions
    {
        Timeout = 60,
        CommandType = CommandType.Text, // ou StoredProcedure
        ConnectionName = "replica", // multi-database
        CacheKey = "users:all",
        CacheExpiration = TimeSpan.FromMinutes(5)
    }
);
```

## Multi-database

```csharp
builder.Services.AddRapidMapper(builder =>
{
    builder.AddConnectionFactory(sp =>
    {
        var factories = new Dictionary<string, Func<IDbConnection>>
        {
            ["primary"] = () => new SqlConnection(connString1),
            ["replica"] = () => new SqlConnection(connString2),
        };
        return new ConnectionFactory(factories, "primary");
    });

    builder.AddProvider<SqlServerProvider>();
});
```

## Providers

### Oficiais

```csharp
builder.AddProvider<SqlServerProvider>();
builder.AddProvider<PostgreSqlProvider>();
```

### Genérico (qualquer banco)

```csharp
// MySQL
builder.AddGenericProvider("MySql", () => new MySqlConnection(cs));

// Oracle
builder.AddGenericProvider("Oracle", () => new OracleConnection(cs));

// SQLite
builder.AddGenericProvider("SQLite", () => new SqliteConnection(cs));
```

### Provider customizado

```csharp
public class MySqlProvider : DatabaseProvider
{
    private readonly string _cs;
    public override string Name => "MySql";
    public MySqlProvider(string cs) => _cs = cs;
    public override IDbConnection CreateConnection() => new MySqlConnection(_cs);
}

builder.AddProvider<MySqlProvider>();
```

## Dialects

```csharp
builder.AddDialect<SqlServerDialect>();
// ou
builder.AddDialect<PostgreSqlDialect>();
```

| Método | SqlServer | PostgreSQL |
|--------|-----------|------------|
| `ApplyPagination(sql, 10, 20)` | `OFFSET 10 ROWS FETCH NEXT 20` | `LIMIT 20 OFFSET 10` |
| `GetIdentityQuery()` | `SELECT SCOPE_IDENTITY()` | `SELECT LASTVAL()` |
| `NormalizeParameter("Id")` | `@Id` | `@Id` |

## OpenTelemetry

Spans automáticos com tags: `db.statement`, `db.system`, `db.row_count`, `db.duration_ms`, `cache.hit`, `error.type`.

```csharp
services.AddOpenTelemetry().WithTracing(b => b.AddSource("RapidMapper"));
```

## Métricas

```csharp
context.Metrics.StartTime        // Início da execução
context.Metrics.EndTime          // Fim da execução
context.Metrics.TotalDuration    // Tempo total
context.Metrics.DatabaseDuration // Tempo do provider
context.Metrics.MappingDuration  // Tempo do mapping
context.Metrics.RowCount         // Linhas afetadas/retornadas
```

## Parâmetros

### Objetos anônimos

```csharp
new { Id = 1, Nome = "João" }
```

### Dictionary

```csharp
new Dictionary<string, object> { ["Id"] = 1, ["Nome"] = "João" }
```

## Pontos de Atenção

- **Target framework**: todos os projetos usam net8.0
- **Behaviors**: use `IOrderedBehavior` para controle explícito de ordem
- **ICacheProvider**: deve ser implementado pelo consumidor (ex: MemoryCache, Redis)
- **IDialect**: SqlServerDialect e PostgreSqlDialect incluídos; MySQL/Oracle requerem implementação customizada
- **Mapping**: Expression Trees para propriedades diretas; fallback para reflection em nested mapping
- **Source Generator**: não implementado (roadmap futuro)
