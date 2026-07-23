# FlowMapper V2

**FlowMapper é uma plataforma de mapeamento de dados em compile-time para .NET.**

Combina mapeamento objeto-objeto, micro-ORM, deserialização, source generation e pipelines de execução em uma arquitetura unificada focada em **performance**, **extensibilidade** e **zero reflection em runtime**.

---

## Índice

1. [Visão Geral](#1-visão-geral)
2. [Instalação](#2-instalação)
3. [Estrutura do Projeto](#3-estrutura-do-projeto)
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
   - [Escopo Transacional](#65-escopo-transacional)
7. [Deserialização](#7-deserialização)
   - [JSON](#71-json)
   - [XML](#72-xml)
   - [TXT / CSV](#73-txt--csv)
8. [Provedores de Banco](#8-provedores-de-banco)
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
15. [Exemplos Completos](#15-exemplos-completos)

---

## 1. Visão Geral

FlowMapper V2 é um framework .NET **all-in-one** que unifica:

| Área | Descrição |
|------|-----------|
| **Object-Object Mapping** | Mapeamento entre objetos (similar a AutoMapper) com perfis, expressões fluent, flatten, nested, construtores |
| **Micro-ORM** | Acesso a dados assíncrono com materialização automática de DTOs aninhados via alias de colunas |
| **Source Generator** | Geração de código em compile-time (`IMapper<,>`) sem reflection em runtime |
| **Deserialização** | JSON, XML e TXT/CSV para DTOs com suporte aninhado |
| **4 Provedores** | SQL Server, PostgreSQL, MySQL, Oracle |
| **DI Integration** | `AddFlowMapper()` com registro fluente de perfis, provedores e comportamentos |

```csharp
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=...;");
});
```

### Arquitetura

```
           SQL / JSON / XML / Object
                     │
                     ▼
         ┌───────────────────────┐
         │   Compiler Pipeline   │
         │  (13 otimizações)     │
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
   (Objeto → Objeto)   (DataReader → Objeto)
         │                       │
         ▼                       ▼
   Runtime Engine ────────── Execution Scope
         │
         ▼
     DTO / Entidade
```

### Por que FlowMapper?

| Benefício | Descrição |
|-----------|-----------|
| ✅ **Mapeamento em Compile-time** | Source generator produz `IMapper<,>` em tempo de build |
| ✅ **Zero Reflection** | Sem `System.Reflection` em hot paths — startup e execução mais rápidos |
| ✅ **Source Generator** | Roslyn `IIncrementalGenerator` — erros aparecem em compile-time |
| ✅ **Native AOT Ready** | Sem geração dinâmica de código — funciona com `nativeaot` |
| ✅ **Nested Mapping** | Mapeamento recursivo objeto-para-objeto e SQL-para-DTO com aliases |
| ✅ **Flatten Mapping** | Auto-flatten `Endereco.Rua` → `EnderecoRua` com separador `_` |
| ✅ **Micro-ORM** | `QueryAsync<T>`, `StreamAsync<T>`, `CommandAsync<T>` com materialização em cascata |
| ✅ **4 Provedores SQL** | SQL Server, PostgreSQL, MySQL, Oracle — cada um com paginação por dialeto |
| ✅ **Execution Pipelines** | Cadeia de middlewares `IPipelineBehavior` para concerns transversais |
| ✅ **Materialization Pipeline** | Middlewares de cache, conversão e tratamento de null |
| ✅ **Validation Pipeline** | Validação baseada em regras com `IValidationRule` |
| ✅ **Diagnostics Pipeline** | Diagnóstico baseado em eventos e middlewares com métricas |
| ✅ **Compiler Pipeline** | 13 passes de otimização (flatten, fusão, constant eval, dead metadata) |
| ✅ **Plugin SDK** | Extenda tudo: provedores, estágios, passes, regras, geradores |
| ✅ **Deserialização** | JSON, XML, TXT/CSV — todos com suporte a DTOs aninhados |
| ✅ **Caching** | 5 níveis: `ICacheProvider`, delegates compilados, flows, planos |

### Comparação

| Funcionalidade | FlowMapper | AutoMapper | Mapster | Dapper |
|----------------|-----------|------------|---------|--------|
| Mapeamento Compile-time | ✅ | ❌ | ✅ | ❌ |
| Source Generator | ✅ | ❌ | ✅ | ❌ |
| Nested Mapping | ✅ | ✅ | ✅ | ❌ |
| Flatten Mapping | ✅ | ✅ | ✅ | ❌ |
| Micro-ORM | ✅ | ❌ | ❌ | ✅ |
| Provedores SQL (4) | ✅ | ❌ | ❌ | ✅ |
| Materialization Pipeline | ✅ | ❌ | ❌ | Parcial |
| Execution Plans | ✅ | ❌ | ❌ | ❌ |
| Plugin SDK | ✅ | ❌ | ❌ | ❌ |
| Diagnostics Pipeline | ✅ | ❌ | ❌ | ❌ |
| Validation Pipeline | ✅ | ❌ | ❌ | ❌ |
| Deserialização (JSON/XML/TXT) | ✅ | ❌ | ❌ | ❌ |
| Native AOT | ✅ | ❌ | ❌ | ❌ |

---

## 2. Instalação

Adicione o pacote NuGet:

```xml
<PackageReference Include="FlowMapper" Version="2.0.0" />
```

**Pré-requisitos:** .NET 8.0+

---

## 3. Estrutura do Projeto

```
src/
├── FlowMapper/                          # Meta-package (referencia todos os sub-projetos)
├── FlowMapper.Abstractions/             # Interfaces públicas (IFlowMapper, IRapidMapper, etc.)
├── FlowMapper.Primitives/               # Value types (ArtifactId, ExecutionId, PipelineId, ProviderId)
├── FlowMapper.Core/                     # Motor de mapeamento objeto-objeto
├── FlowMapper.Execution/                # Modelo de execution plan e artifacts
├── FlowMapper.Mapping/                  # Pipeline de mapeamento com middlewares
├── FlowMapper.Materializer/             # Pipeline de materialização DataReader → objeto
├── FlowMapper.Compiler/                 # Pipeline de compilação com 13 passes de otimização
├── FlowMapper.Runtime/                  # Implementações runtime (query, command, stream executors)
├── FlowMapper.Deserialization/          # Deserialização JSON, XML, TXT
├── FlowMapper.Diagnostics/              # Pipeline de diagnósticos com middlewares
├── FlowMapper.Validation/               # Pipeline de validação baseada em regras
├── FlowMapper.SqlCompiler/              # Pipeline de compilação SQL com middlewares
├── FlowMapper.SourceGenerator/          # Gerador de código Roslyn (compile-time)
├── FlowMapper.DependencyInjection/      # Integração DI (AddFlowMapper)
├── FlowMapper.Providers.Abstractions/   # Abstrações de provedores de banco
├── FlowMapper.Providers.SqlServer/      # Provedor SQL Server
├── FlowMapper.Providers.PostgreSql/     # Provedor PostgreSQL
├── FlowMapper.Providers.MySql/          # Provedor MySQL
├── FlowMapper.Providers.Oracle/         # Provedor Oracle
├── FlowMapper.PluginSdk/                # SDK de plugins (IFlowMapperPlugin)
├── FlowMapper.Analyzers/                # Analisadores Roslyn
├── FlowMapper.BuildIntegration/         # Integração com MSBuild
├── FlowMapper.SqlCompiler/              # Compilador SQL
├── FlowMapper.Mapping/                  # Pipeline de mapeamento
├── FlowMapper.Cli/                      # Ferramenta CLI
├── FlowMapper.Data/                     # Utilitários de dados
├── FlowMapper.Csv/                      # Suporte a CSV
```

---

## 4. Object-Object Mapping

### 4.1 ProfileDefinition

Classe abstrata para definição de perfis de mapeamento:

```csharp
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        ProfileName = "AppProfile";

        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} ({s.Email})"));

        CreateMap<Pedido, PedidoDto>()
            .ReverseMap()
            .DisableFlattenMapping();
    }
}
```

**Propriedades do perfil:**

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `ProfileName` | `string` | `"Default"` | Nome do perfil |
| `Policy` | `MappingPolicy` | — | Política global (strictness, flatten, constructor) |

**MappingPolicy:**

| Propriedade | Tipo | Padrão | Descrição |
|-------------|------|--------|-----------|
| `Strictness` | `StrictnessLevel` | `Warning` | Comportamento ao encontrar propriedades não mapeadas |
| `EnableFlatten` | `bool` | `true` | Ativar flatten automático |
| `PreferConstructor` | `bool` | `false` | Preferir mapeamento via construtor |

**StrictnessLevel:**

| Valor | Descrição |
|-------|-----------|
| `None` | Ignorar propriedades não mapeadas |
| `Warning` | Apenas avisar |
| `Error` | Lançar erro |

### 4.2 MappingExpression

Retornado por `CreateMap<T1, T2>()`, oferece a API fluente:

```csharp
CreateMap<Source, Dest>()
    .ForMember(d => d.Propriedade, opt => opt.MapFrom(s => s.Outra))
    .ForPath(d => d.Endereco.Rua, opt => opt.MapFrom(s => s.Endereco.Logradouro))
    .ReverseMap()
    .ConstructUsing(s => new Dest(s.Id, s.Nome))
    .DisableFlattenMapping();
```

### 4.3 ForMember / ForPath

```csharp
// ForMember — mapeamento de propriedade simples
CreateMap<User, UserDto>()
    .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.FirstName} {s.LastName}"))
    .ForMember(d => d.Senha, opt => opt.Ignore());

// ForPath — mapeamento de propriedade aninhada
CreateMap<Order, OrderDto>()
    .ForPath(d => d.ClienteNome, opt => opt.MapFrom(s => s.Customer.Name));
```

**MemberOptions:**

| Método | Descrição |
|--------|-----------|
| `MapFrom(string sourceProperty)` | Mapear de uma propriedade por nome |
| `MapFrom(Expression<Func<TSource, object?>>)` | Mapear de uma expressão |
| `Ignore()` | Ignorar a propriedade no mapeamento |

### 4.4 ReverseMap

Cria automaticamente o mapeamento inverso:

```csharp
CreateMap<User, UserDto>()
    .ReverseMap();
// Equivalente a CreateMap<UserDto, User>() com as mesmas configurações invertidas
```

### 4.5 ConstructUsing

Define uma fábrica personalizada para criar o objeto de destino:

```csharp
// Método de fábrica
CreateMap<Input, Output>()
    .ConstructUsing(CreateOutput);

static Output CreateOutput(Input source) => new(source.Value * 2);

// Lambda inline
CreateMap<Input, Output>()
    .ConstructUsing(source => new Output(source.Value * 2));
```

### 4.6 Flatten Mapping

**Ativado por padrão.** Transforma propriedades aninhadas em propriedades "achatadas" usando separador `_`:

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

Para desabilitar por mapeamento:

```csharp
CreateMap<Employee, EmployeeDto>()
    .DisableFlattenMapping();
```

### 4.7 Nested Mapping

Detectado automaticamente para tipos complexos (class, não-string, não-value-type, não-IEnumerable):

```csharp
public class Order
{
    public Customer Customer { get; set; }
}

public class OrderDto
{
    public CustomerDto Customer { get; set; }
}
// Order.Customer.Name → OrderDto.Customer.Name (recursivo)
```

### 4.8 Constructor Mapping

Suporte a tipos imutáveis (records) e `ConstructUsing`:

```csharp
// Record com construtor primário
public record PersonDto(int Id, string Name);

// O Source Generator detecta automaticamente os parâmetros do construtor
```

### 4.9 AfterMap / BeforeMap

Callbacks executados após/antes do mapeamento:

```csharp
// Method group
CreateMap<Order, OrderDto>()
    .AfterMap(CalculateTotals);

static void CalculateTotals(Order source, OrderDto target)
{
    target.Total = source.Price * source.Quantity;
}

// Lambda inline
CreateMap<Order, OrderDto>()
    .AfterMap((source, target) => target.Total = source.Price * source.Quantity);
```

---

## 5. Source Generator

O **FlowMapperGenerator** é um `IIncrementalGenerator` Roslyn que gera implementações de `IMapper<,>` em **compile-time**, eliminando reflection em runtime.

**Benefícios do Source Generator:**

| Benefício | Descrição |
|-----------|-----------|
| ⚡ **Sem reflection em runtime** | Todo o mapeamento é código C# compilado — sem `System.Reflection` em hot paths |
| 🚀 **Startup mais rápido** | Sem compilação de expressões em runtime; o código já está pronto |
| 🔍 **Debugging facilitado** | Você pode depurar o código gerado como qualquer outra classe |
| ✅ **Validação em compile-time** | Erros de mapeamento aparecem na compilação, não em produção |
| 📦 **Menos alocações** | Código otimizado sem delegates dinâmicos ou dicionários de reflection |
| 🏗️ **Native AOT friendly** | Sem geração dinâmica de IL — compatível com `nativeaot` publish |

**Como funciona:**

1. Detecta classes que herdam de `ProfileDefinition`
2. Escaneia chamadas `CreateMap<>()` dentro dos construtores
3. Extrai mapeamentos explícitos, `ForMember`, `ForPath`, `AfterMap`, `ConstructUsing`
4. Gera código C# com implementações completas de `IMapper<,>`

**Saída:** `FlowMapper_Mappers.g.cs` com mapeadores compilados.

**Exemplo de código gerado:**

```csharp
// Gerado automaticamente
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

O Source Generator também suporta o atributo `[Map<T1, T2>]` (legado):

```csharp
[Map<User, UserDto>]
public partial class UserMapper : IMapper<User, UserDto>;
```

---

## 6. Micro-ORM

### 6.1 QueryAsync

Retorna múltiplas linhas mapeadas para DTOs:

```csharp
var usuarios = await flow.QueryAsync<UsuarioDto>(
    "SELECT Id, Nome, Email FROM Usuario WHERE Ativo = @Ativo",
    new { Ativo = true });
```

**DTOs aninhados via alias:**

```csharp
var dados = await flow.QueryAsync<ClienteDto>(@"
    SELECT
        u.Id,
        u.Nome,
        p.Id   AS Perfil_Id,
        p.Nome AS Perfil_Nome
    FROM Usuario u
    LEFT JOIN Perfil p ON p.UsuarioId = u.Id");
// → ClienteDto.Perfil.Id e ClienteDto.Perfil.Nome preenchidos
```

### 6.2 QuerySingleAsync

Retorna exatamente uma linha (lança exceção se não encontrar):

```csharp
var usuario = await flow.QuerySingleAsync<UsuarioDto>(
    "SELECT Id, Nome FROM Usuario WHERE Id = @Id",
    new { Id = 1 });
```

### 6.3 StreamAsync

Streaming assíncrono de resultados (não carrega tudo em memória):

```csharp
await foreach (var usuario in flow.StreamAsync<UsuarioDto>(
    "SELECT Id, Nome FROM Usuario"))
{
    Processar(usuario);
}
```

### 6.4 CommandAsync

Executa comandos (INSERT, UPDATE, DELETE) e retorna linhas afetadas:

```csharp
var afetados = await flow.CommandAsync(
    "UPDATE Usuario SET Nome = @Nome WHERE Id = @Id",
    new { Id = 1, Nome = "João" });

var novoId = await flow.CommandScalarAsync<int>(
    "INSERT INTO Usuario (Nome) VALUES (@Nome); SELECT SCOPE_IDENTITY()",
    new { Nome = "Maria" });
```

### 6.5 Escopo Transacional

```csharp
await using var scope = flow.CreateScope(transactional: true);

await flow.CommandAsync("INSERT INTO Pedido ...", new { ... });
await flow.CommandAsync("UPDATE Estoque ...", new { ... });

await scope.CommitAsync();
// Se não chamar CommitAsync(), faz Rollback no DisposeAsync()
```

---

## 7. Deserialização

### 7.1 JSON

Suporte a objetos e listas com DTOs aninhados:

```csharp
// Objeto único
var dto = flow.FromJson<ClienteDto>(@"{
    ""Id"": 1,
    ""Nome"": ""Maria"",
    ""Perfil"": { ""Id"": 10, ""Nome"": ""Admin"" }
}");

// Lista
var lista = flow.FromJsonList<Usuario>(jsonArray);
```

### 7.2 XML

```csharp
var dto = flow.FromXml<ClienteDto>(@"
    <ClienteDto>
        <Id>1</Id>
        <Nome>Pedro</Nome>
        <Perfil>
            <Id>10</Id>
            <Nome>Admin</Nome>
        </Perfil>
    </ClienteDto>");
```

### 7.3 TXT / CSV

Suporte a delimitadores `;` (PontoVirgula) e `\t` (Tabulacao), com ou sem cabeçalho:

```csharp
// CSV com cabeçalho (case-insensitive)
var lista = flow.FromText<ClienteCsvDto>(csvLines,
    TextDelimiter.PontoVirgula,
    hasHeader: true);

// CSV posicional (sem cabeçalho — mapeia pela ordem das colunas)
var lista = flow.FromText<Usuario>(lines,
    TextDelimiter.PontoVirgula,
    hasHeader: false);
```

---

## 8. Provedores de Banco

### 8.1 SQL Server

| Propriedade | Valor |
|-------------|-------|
| Classe | `SqlServerProvider` |
| Connection | `SqlConnection` |
| Paginação | `OFFSET x ROWS FETCH NEXT y ROWS ONLY` |
| Identity | `SELECT SCOPE_IDENTITY()` |
| Parâmetros | `@nome` |

```csharp
builder.AddProvider<SqlServerProvider>("Server=localhost;Database=MeuDb;Trusted_Connection=True;");
```

### 8.2 PostgreSQL

| Propriedade | Valor |
|-------------|-------|
| Classe | `PostgreSqlProvider` |
| Connection | `NpgsqlConnection` |
| Paginação | `LIMIT y OFFSET x` |
| Identity | `SELECT LASTVAL()` |
| Parâmetros | `@nome` |

```csharp
builder.AddProvider<PostgreSqlProvider>("Host=localhost;Database=MeuDb;Username=user;Password=pass;");
```

### 8.3 MySQL

| Propriedade | Valor |
|-------------|-------|
| Classe | `MySqlProvider` |
| Connection | `MySqlConnection` |
| Paginação | `LIMIT y OFFSET x` |
| Identity | `SELECT LAST_INSERT_ID()` |
| Parâmetros | `@nome` |

```csharp
builder.AddProvider<MySqlProvider>("Server=localhost;Database=MeuDb;Uid=root;Pwd=pass;");
```

### 8.4 Oracle

| Propriedade | Valor |
|-------------|-------|
| Classe | `OracleProvider` |
| Connection | `OracleConnection` |
| Paginação | Subquery com ROWNUM |
| Identity | `SELECT LAST_INSERT_ID()` |
| Parâmetros | `:nome` |

```csharp
builder.AddProvider<OracleProvider>("Data Source=localhost:1521/MeuDb;User Id=user;Password=pass;");
```

---

## 9. DI Integration

Registra todos os serviços automaticamente:

```csharp
services.AddFlowMapper(builder =>
{
    // Perfil de mapeamento
    builder.AddProfile<AppProfile>();

    // Provedor de banco
    builder.AddProvider<SqlServerProvider>("Server=...;");

    // Configurações globais
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

**Serviços registrados automaticamente:**

| Serviço | Implementação | Lifetime |
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

Pipeline de execução estilo HTTP-middleware com `IPipelineBehavior`:

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

// Registro
services.AddFlowMapper();
services.AddTransient<IPipelineBehavior, LoggingBehavior>();
```

**ExecutionPhase:**

| Fase | Descrição |
|------|-----------|
| `BeforeExecute` | Antes da execução |
| `Execute` | Execução do comando/query |
| `Mapping` | Mapeamento dos resultados |
| `RowRead` | Leitura de cada linha |
| `AfterExecute` | Após a execução |
| `Completed` | Finalizado |

### 10.2 Mapping Pipeline

Pipeline interno para mapeamento objeto-objeto:

| Middleware | Descrição |
|------------|-----------|
| `NullPropagationMiddleware` | Lança `ArgumentNullException` se source for null |
| `FlattenMiddleware` | Suporte a flatten mapping |

### 10.3 Materialization Pipeline

Pipeline para materialização de `DataReader` → objeto:

| Middleware | Descrição |
|------------|-----------|
| `CachingMiddleware` | Cache de delegates compilados |
| `ConversionMiddleware` | Conversão de tipos |
| `NullValueHandlingMiddleware` | Tratamento de DBNull |

### 10.4 Validation Pipeline

Pipeline de validação baseada em regras:

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

Pipeline de diagnóstico baseado em eventos e middlewares:

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

Pipeline de compilação SQL:

| Middleware | Descrição |
|------------|-----------|
| `DialectMiddleware` | Aplica dialeto (paginação, parâmetros) |
| `ParameterNormalizationMiddleware` | Normaliza nomes de parâmetros |
| `SqlCachingMiddleware` | Cache de SQL compilado |

### 10.7 Compiler Pipeline

Pipeline de compilação com estágios:

1. **MetadataStage** — Constrói metadados dos tipos
2. **OptimizationStage** — Aplica 13 passes de otimização
3. **ValidationStage** — Valida os artefatos
4. **ExecutionPlanStage** — Constrói planos de execução
5. **SourceGenerationStage** — Gera código fonte

**Passes de Otimização:**

| Pass | Descrição |
|------|-----------|
| `FlattenOptimizationPass` | Otimiza flatten mapping |
| `NestedOptimizationPass` | Otimiza nested mapping |
| `DelegateFusionPass` | Fusiona múltiplos delegates em um |
| `ConstantEvaluationPass` | Avalia expressões constantes |
| `NullOptimizationPass` | Otimiza verificações de null |
| `ConverterOptimizationPass` | Otimiza conversores de tipo |
| `RedundantMappingRemovalPass` | Remove mapeamentos redundantes |
| `DeadMetadataEliminationPass` | Elimina metadados não utilizados |

---

## 11. Plugin SDK

Sistema completo de plugins com 7 categorias:

```csharp
public class MeuPlugin : IFlowMapperPlugin, ICompilerPlugin
{
    public string Name => "MeuPlugin";
    public Version Version => new(1, 0);

    public void Configure(IPluginBuilder builder)
    {
        builder.AddOptimizationPass(typeof(MeuOptimizationPass));
        builder.AddCompilerStage(typeof(MeuStage));
    }

    public IReadOnlyCollection<Type> GetStageTypes() => new[] { typeof(MeuStage) };
}
```

**Interfaces de Marcadores:**

| Interface | Categoria |
|-----------|-----------|
| `ICompilerPlugin` | Adiciona estágios ao compiler pipeline |
| `IProviderPlugin` | Provedor de banco |
| `IRuntimePlugin` | Serviços runtime |
| `IValidationPlugin` | Regras de validação |
| `IOptimizationPlugin` | Passes de otimização |
| `IDiagnosticsPlugin` | Middlewares de diagnóstico |
| `ISourceGeneratorPlugin` | Geradores de código fonte |

---

## 12. Caching

Múltiplos níveis de cache:

| Nível | Local | Descrição |
|-------|-------|-----------|
| `ICacheProvider` | Abstração | Interface para cache externo (Redis, MemoryCache, etc.) |
| `CachingMiddleware` | Materialization | Cache de delegates compilados (`MaterializationDelegate<T>`) |
| `FlowCache` | Source Generator | Cache de flows construídos |
| `FlowBuilder` | Core | Cache de `Flow` objects em `ConcurrentDictionary<(Type, Type), Flow>` |
| `ExecutionOptions` | Runtime | Cache opcional por chave via `CacheKey`/`CacheExpiration` |

```csharp
// Cache por execução
var resultado = await flow.QueryAsync<UsuarioDto>(sql,
    new { Ativo = true },
    new ExecutionOptions { CacheKey = "usuarios-ativos", CacheExpiration = TimeSpan.FromMinutes(5) });

// Cache provider customizado
public class RedisCacheProvider : ICacheProvider
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) { ... }
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default) { ... }
    public Task RemoveAsync(string key, CancellationToken ct = default) { ... }
}
```

---

## 13. Primitives

Value types fortemente tipados:

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

Artefatos de execução produzidos pelo Compiler Pipeline:

| Artefato | Descrição |
|----------|-----------|
| `IMappingArtifact` | Contém `MappingDelegate`, `ReverseMappingDelegate`, `BeforeMapDelegate`, `AfterMapDelegate` |
| `IMaterializationArtifact` | Contém `MaterializationDelegate`, `ColumnBindings`, `ConstructorDelegate` |
| `ISqlArtifact` | Contém `CommandText`, `Parameters`, `ExecutionDelegate` |
| `IProviderArtifact` | Contém `ProviderName`, `ProviderVersion` |
| `IMetadataArtifact` | Contém `ITypeInfo` collection |
| `IConstructorArtifact` | Contém `FactoryDelegate`, `ConstructorParameterBindings` |
| `IExecutionPlan` | Agrupa múltiplos artefatos com um `ExecutionDelegate` |

---

## 15. Exemplos Completos

### Exemplo 1: Mapeamento Básico

```csharp
// 1. Defina o perfil
public class AppProfile : ProfileDefinition
{
    public AppProfile()
    {
        CreateMap<Usuario, UsuarioDto>()
            .ForMember(d => d.NomeCompleto, opt => opt.MapFrom(s => $"{s.Nome} ({s.Email})"));
    }
}

public class Usuario { public int Id { get; set; } public string Nome { get; set; } public string Email { get; set; } }
public class UsuarioDto { public int Id { get; set; } public string NomeCompleto { get; set; } }

// 2. Configure DI
services.AddFlowMapper(builder => builder.AddProfile<AppProfile>());
var flow = services.BuildServiceProvider().GetRequiredService<IFlowMapper>();

// 3. Mapeie
var dto = flow.Map<Usuario, UsuarioDto>(new Usuario { Id = 1, Nome = "João", Email = "joao@email.com" });
```

### Exemplo 2: Micro-ORM Completo

```csharp
services.AddFlowMapper(builder =>
{
    builder.AddProfile<AppProfile>();
    builder.AddProvider<SqlServerProvider>("Server=localhost;Database=Loja;Trusted_Connection=True;");
});

var flow = serviceProvider.GetRequiredService<IFlowMapper>();

// Query com DTO aninhado
var pedidos = await flow.QueryAsync<PedidoDto>(@"
    SELECT
        p.Id,
        p.Total,
        c.Id   AS Cliente_Id,
        c.Nome AS Cliente_Nome
    FROM Pedido p
    JOIN Cliente c ON c.Id = p.ClienteId
    WHERE p.Total > @Minimo",
    new { Minimo = 100.0m });

// Insert com retorno de ID
var novoId = await flow.CommandScalarAsync<int>(
    "INSERT INTO Produto (Nome, Preco) VALUES (@Nome, @Preco); SELECT SCOPE_IDENTITY();",
    new { Nome = "Teclado", Preco = 199.90m });

// Transação
await using var scope = flow.CreateScope(transactional: true);
await flow.CommandAsync("INSERT INTO Pedido (ClienteId, Total) VALUES (@ClienteId, @Total)", new { ClienteId = 1, Total = 50.0m });
await flow.CommandAsync("UPDATE Produto SET Estoque = Estoque - 1 WHERE Id = @Id", new { Id = 5 });
await scope.CommitAsync();
```

### Exemplo 3: Deserialização Multi-formato

```csharp
var flow = serviceProvider.GetRequiredService<IFlowMapper>();

// JSON com aninhamento
var json = """{"Id":1,"Nome":"Maria","Perfil":{"Id":10,"Nome":"Admin"}}""";
var dto = flow.FromJson<ClienteDto>(json);

// XML
var xml = """<ClienteDto><Id>1</Id><Nome>Pedro</Nome><Perfil><Id>10</Id><Nome>Admin</Nome></Perfil></ClienteDto>""";
var dto2 = flow.FromXml<ClienteDto>(xml);

// CSV
var csv = new[] { "Id;Nome;Email", "1;João;joao@email.com", "2;Maria;maria@email.com" };
var lista = flow.FromText<UsuarioDto>(csv, TextDelimiter.PontoVirgula, hasHeader: true);
```

### Exemplo 4: Pipeline Behavior Customizado

```csharp
public class LoggingBehavior : IPipelineBehavior, IOrderedBehavior
{
    public int Order => 0;

    public bool ShouldExecute<T>(ExecutionContext<T> context) => true;

    public async Task HandleAsync<T>(ExecutionContext<T> context, Func<Task> next)
    {
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Tipo={typeof(T).Name} SQL={context.Sql}");
        var sw = Stopwatch.StartNew();
        try
        {
            await next();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ERRO] {ex.Message}");
            throw;
        }
        finally
        {
            sw.Stop();
            Console.WriteLine($"[{sw.ElapsedMilliseconds}ms] Concluído");
        }
    }
}

// Registro
services.AddFlowMapper();
services.AddSingleton<IPipelineBehavior, LoggingBehavior>();
```

### Exemplo 5: Plugin

```csharp
public class MeuProviderPlugin : IFlowMapperPlugin, IProviderPlugin
{
    public string Name => "MeuDB";
    public Version Version => new(1, 0);

    public void Configure(IPluginBuilder builder)
    {
        builder.AddProvider<MeuProvider>();
    }
}

// Carregar plugins
var loader = new PluginLoader();
loader.LoadFromAssembly(typeof(MeuProviderPlugin).Assembly);
```

---

## Ecossistema

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

FlowMapper faz parte de um ecossistema .NET em crescimento. O design modular permite que cada camada seja usada independentemente.

---

## Roadmap

### Versão 2.0
- ✅ Object-Object Mapping com API fluente
- ✅ Source Generator (compile-time `IMapper<,>`)
- ✅ Micro-ORM com materialização aninhada
- ✅ 4 Provedores SQL (SQL Server, PostgreSQL, MySQL, Oracle)
- ✅ Deserialização JSON, XML, TXT
- ✅ DI Integration (`AddFlowMapper`)

### Versão 2.1
- ✅ Plugin SDK
- ✅ Compiler Pipeline com 13 passes de otimização
- ✅ Diagnostics Pipeline
- ✅ Validation Pipeline
- ✅ Execution Artifacts

### Futuro
- 🔲 Query Optimizer
- 🔲 Novos Provedores (SQLite, Cosmos DB)
- 🔲 Melhorias nos Analisadores Roslyn
- 🔲 Ferramenta CLI de scaffolding

---

## Licença

MIT
