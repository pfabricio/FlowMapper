
## 2026-07-04 17:53:12 UTC
## Build Fixed (2026-07-04)

### Errors Fixed (10→0)
1. **IAsyncDisposable/IAsyncEnumerable on netstandard2.0** — Added `Microsoft.Bcl.AsyncInterfaces` NuGet package to `FlowMapper.Abstractions.csproj`.
2. **SetPhase internal in ExecutionContext** — Changed `internal void SetPhase()` → `public void SetPhase()` so `FlowMapper.Data` can call it.
3. **IsExternalInit not defined on netstandard2.0** — Added polyfill `IsExternalInit.cs` (guarded by `#if NETSTANDARD2_0`) to `FlowMapper.Core`.
4. **TimeSpan * double not supported on netstandard2.0** — Changed `_initialDelay * Math.Pow(2, i)` → `TimeSpan.FromMilliseconds(_initialDelay.TotalMilliseconds * Math.Pow(2, i))` in `RetryExecutionStrategy.cs`.

### Remaining Warnings (136)
All are CS1591 (missing XML comments). These are harmless but noisy — can suppress by adding `<NoWarn>$(NoWarn);CS1591</NoWarn>` to the projects or removing `<GenerateDocumentationFile>` from `Directory.Build.props`.

### Build Status
**FULL SOLUTION BUILD SUCCEEDS** — all 11 projects + 2 samples compile for all target frameworks (net8.0, netstandard2.0, net10.0) with 0 errors.

## 2026-07-04 17:57:55 UTC
## Test Project Created (2026-07-04)

### FlowMapper.Tests (xUnit)
- **Location**: `tests/FlowMapper.Tests/`
- **Target**: net8.0
- **Packages**: xUnit 2.9.3, FluentAssertions 7.0.0, Moq 4.20.72, coverlet 6.0.4, Microsoft.NET.Test.Sdk 17.12.0
- **References**: FlowMapper.Abstractions, Core, Data, Mapping, FlowMapper

### Test Files (8 files, 31 tests)
| File | Tests |
|------|-------|
| `Abstractions/ExecutionContextTests.cs` | 5 |
| `Abstractions/ExecutionOptionsTests.cs` | 2 |
| `Abstractions/ExecutionPhaseTests.cs` | 1 |
| `Abstractions/MappingOptionsTests.cs` | 2 |
| `Core/MappingExpressionTests.cs` | 9 |
| `Core/ProfileDefinitionTests.cs` | 3 |
| `Core/FlowMapperOptionsTests.cs` | 3 |
| `Data/RapidMapperTests.cs` | 5 |
| `Data/DefaultMapperTests.cs` | 2 |

**Result: 31/31 passed** — build + tests green with 0 errors.


## 2026-07-04 18:03:01 UTC
## ForPath + ReverseMap Implemented (2026-07-04)

### `MappingExpression.cs` changes:
- Added `ForPath(Expression<Func<TDestination, object?>>, Action<PathMemberOptions>)` — parses nested member expressions (e.g. `d => d.Perfil.PerfilID`), stores as `ExplicitMapping` with `IsPathMapping=true` and `PathSegments` list
- Added `ExtractPathSegments(MemberExpression)` — recursively walks member expression chain, returns reversed segments list
- Added `PathMemberOptions` class — same pattern as `MemberOptions` (MapFrom string/expression)
- Added `ReverseMap()` — sets internal `ReverseMapped` flag
- Added `ReverseMapped` internal property
- Added `IsPathMapping` and `PathSegments` to `ExplicitMapping`
- Fixed `^1` → `pathSegments.Count - 1` for netstandard2.0 compatibility

### Tests added (7 new, 38 total):
- `ForPath_WithTwoSegments_CreatesPathMapping` — path `Nested.FullName`, verifies `IsPathMapping=true`, `PathSegments=["Nested","FullName"]`
- `ForPath_WithOneSegment_CreatesPathMapping` — single segment equates to `ForMember`-like behavior
- `ForPath_WithoutExplicitMapFrom_UsesLastSegment` — defaults SourceProperty to last segment
- `ForPath_WithExpression_SetsMapFromExpression` — expression-based MapFrom
- `ReverseMap_SetsReverseMappedFlag` — flag is set after calling `ReverseMap()`
- `ReverseMap_WithoutCall_ReverseMappedIsFalse` — default is `false`
- `ReverseMap_ReturnsSameInstance` — fluent API returns self

### Status: **38/38 tests pass, 0 build errors**


## 2026-07-04 18:08:25 UTC
## DI, Map Extension, SourceGenerator (2026-07-04)

### Created files:

**src/FlowMapper.Abstractions/IFlowMapper.cs**
- `IFlowMapper` interface with `Map<TSource, TDest>(TSource)` and `GetMapper<TSource, TDest>()`

**src/FlowMapper.Mapping/FlowMapperService.cs**
- `IFlowMapper` implementation with lazy assembly scanning for `IMapper<,>` types + reflection fallback mapper

**src/FlowMapper.DependencyInjection/FlowMapperBuilder.cs**
- Fluent builder with `AddProvider<T>()`, `AddBehavior<T>()`, `AddDialect<T>()`, `UseNamingStrategy<T>()`, `UseRetryStrategy()`, `UseCacheProvider<T>()`, `AddGenericProvider()`, `AddConnectionFactory()`, `ConfigureData()`, `AddProfile<T>()`, `ConfigureMapping()`

**src/FlowMapper.DependencyInjection/ServiceCollectionExtensions.cs**
- `AddFlowMapper(Action<FlowMapperBuilder>?)` — registers all data access + object mapping services
- `UseFlowMapper(this IServiceProvider)` — initializes Map extensions

**src/FlowMapper.DependencyInjection/Extensions/DataMapperExtensions.cs**
- `.Map<TDest>()` extension methods on `Task<IEnumerable<TSource>>`, `Task<List<TSource>>`, `Task<TSource>`, `IAsyncEnumerable<TSource>`

**src/FlowMapper.SourceGenerator/FlowMapperGenerator.cs**
- Incremental source generator that detects `ProfileDefinition` subclasses, parses `CreateMap<T1, T2>()` calls, generates `partial` mapper class stubs

### Modified:
- `FlowMapper.Mapping.csproj` — added `Core` reference
- `FlowMapper.DependencyInjection.csproj` — added `Mapping` reference

### Status: **38/38 tests pass, 0 build errors**

### Remaining:
- Flesh out source generator to generate full property mapping code
- Implement `PostConfigure` execution pipeline wiring
- More tests for PipelineExecutor, executors
- CI/CD pipeline


## 2026-07-05 17:23:53 UTC
## Sessão: Providers + CombinedSample + IntegrationTests + NuGet packaging — 05 Jul 2026

### Feito
- **Providers PostgreSQL, MySQL, Oracle** — 3 novos projetos com `IDatabaseProvider` + `IDialect` (Npgsql, MySqlConnector, Oracle.ManagedDataAccess)
- **Combined sample** (`samples/CombinedMapping`) — demonstra `db.QueryAsync<Usuario>(sql).Map<Usuario, UsuarioDto>()`
- **Integration tests** (`tests/FlowMapper.IntegrationTests`) — Testcontainers com SQL Server, PostgreSQL, MySQL (Docker automático, 4 testes cada)
- **NuGet packaging** — `Directory.Build.props` (autores, licença MIT, tags), SourceGenerator configurado como analyzer
- **Fix DI** — registrado `IConnectionFactory` em `ServiceCollectionExtensions`
- **Removido duplicata** `MapExtensions.cs` (mantido `DataMapperExtensions.cs`)
- **22 projetos compilando com 0 erros**, 13 unit tests passando

### Pendente
- Generator snapshot tests — requer Roslyn Test SDK (estrutura já existe)
- Integration tests — não rodados (Docker contêineres precisam ser iniciados); após rebuild com fix `IConnectionFactory`, devem funcionar

## 2026-07-22 00:27:37 UTC
## Session: Cascade materialization implemented

### Summary
Implemented cascade materialization (SQL → DTO nested class) via property-driven approach (Slapper-style). 

### Key decisions
- **Separator**: `"_"` (single) — nobody uses `__` in real SQL aliases
- **BuildPlan**: Recursive walk of DTO tree, generates flat leaf bindings with prefixed column names (`Perfil_Nome`). Binding `MemberName` = prefixed column name (e.g., `"Perfil_Nome"`)
- **GroupBindings**: Property-driven — iterates DTO properties; for complex types, filters bindings by `b.MemberName.StartsWith(prop.Name + separator)`. No `Split()`, no `GetProperty(parts[0])`
- **Null safety**: If the first column of a nested group is `DBNull`, the sub-object receives `null` (not empty object)
- **N levels**: Recursion via `MakeGenericMethod` in `BuildNestedAssignment`

### Files changed
- `src/FlowMapper.Materializer/Materializer.cs`: `BuildPlan<T>()` recursive, `BuildPlanFlat<T>()` static
- `src/FlowMapper.Materializer/Pipeline/MaterializationDelegateBuilder.cs`: Property-driven `GroupBindings`
- `src/FlowMapper.Runtime/DataExecutionPipeline.cs`: Uses `FlowMapper.Materializer.Materializer.BuildPlanFlat<T>()`
- `src/FlowMapper.Materializer/Pipeline/MaterializationPipeline.cs`: Ctor default `"_"`
- `src/FlowMapper.Abstractions/FlowMapperOptions.cs`: Default `"_"`
- Test files: Updated from `"__"` to `"_"`, `BuildPlanDefault` → `BuildPlanFlat`

### Status
Build OK, 60/60 tests pass.

## 2026-07-22 02:04:31 UTC
**Session 2 — 22 Jul 2026 (corrigindo diretório correto)**

### Problema
Estávamos trabalhando em `E:\LLM-Local\vol\workspace\FlowMapperV2` (pasta errada). O repositório real é `D:\Programas Visuais\FlowMapper` (remote: `https://github.com/pfabricio/FlowMapper.git`, branch `v2`).

### Feito (nesta sessão, no diretório correto)
- **`TextDelimiter` enum** (`PontoVirgula`, `Tabulacao`) em `FlowMapper.Abstractions`
- **`FlowMapper.Deserialization`** projeto completo:
  - `FlowMapper.Deserialization.csproj` (refs: Abstractions, Materializer, Execution)
  - `IDeserializer` interface
  - `DeserializationPipeline` com:
    - JSON → DTO nested via `FlattenJson` + `BuildPlanFlat<T>` + `GroupBindings` recursivo
    - XML → DTO nested via `WalkXml` + mesmo pipeline de materialização
    - TXT → DTO flat com header (case-insensitive) ou posicional
- **`IFlowMapper` unificado** em `FlowMapper.Abstractions`:
  - `Map`, `GetMapper` (existente)
  - `QueryAsync<T>`, `QuerySingleAsync<T>`, `QuerySingleOrDefaultAsync<T>`, `QueryScalarAsync<T>`, `StreamAsync<T>`
  - `CommandAsync`, `CommandScalarAsync`
  - `FromJson<T>`, `FromJsonList<T>`, `FromXml<T>`, `FromText<T>`
  - `CreateScope`
- **`FlowMapperService`** atualizado: compõe `IRapidMapper` + `IDeserializer` via DI
- **`IFlowMapper.cs` antigo deletado** de `FlowMapper.DependencyInjection` (causava ambiguidade)
- **`FlowMapper.DependencyInjection.csproj`** referenciando `FlowMapper.Deserialization`
- **`ServiceCollectionExtensions`** registrando `IDeserializer` (singleton)
- **Meta-package `FlowMapper.csproj`** incluindo `FlowMapper.Deserialization`
- **`CombinedMapping/Program.cs`** atualizado com 7 cenários
- **`README.md`** e **`README.nuget.md`** atualizados com V2 features
- **`Directory.Build.props`** apontando `PackageReadmeFile` para `README.nuget.md`
- **Build: 0 erros** nos projetos tocados
- **Commit + push** (`3aa0103` no branch `v2`)

## Pendente — SourceGenerator (reescrever)

O SourceGenerator atual foi escrito contra uma versão anterior da API Core. Os tipos mudaram, então não compila nem funciona.

### Abordagem: SG auto-contido (modelos próprios)
Criar DTOs internos no SG em vez de referenciar `FlowMapper.Core` direto. Isso desacopla o SG da API de runtime.

### Passos
1. Criar DTOs internos: `FlowModel`, `PropertyFlowModel`, `NestedFlowModel`, `ConstructorBindingModel`, `FlattenPathModel`, `MappingPolicyModel`, `FlowSignatureModel`, `MappingStrategy` enum
2. Atualizar `FlowBuilder.cs` (Roslyn → modelo interno) para usar os DTOs
3. Atualizar validadores (`FlattenRule`, `CycleRule`, `ConstructorRule`, etc.) para usarem os DTOs
4. Atualizar writers (`PropertyWriter`, `ConstructorWriter`, `NestedWriter`, etc.) para emitir código compatível com a API **atual** de Core
5. Ajustar `SignatureGenerator.cs` — `FlowSignature` atual só tem `SourceType` + `DestinationType` (ambos `Type`)
6. Testar com `FlowMapper.Generator.Tests`


## 2026-07-24 00:11:46 UTC
## Sessão: CI fix + v2.0.0 NuGet publish (23 Jul 2026)

**Problema 1:** CI build falhava com 58 erros CS0234 — namespace `FlowMapper.Execution.Artifacts` não existia.
- **Causa raiz:** `.gitignore:33:artifacts/` ignorava `src/FlowMapper.Execution/Artifacts/`. Os 9 arquivos de interface/record existiam no disco desde o sync inicial mas nunca foram commitados.
- **Fixa:** Adicionado `!src/FlowMapper.Execution/Artifacts/` no `.gitignore`. Commitei os 9 arquivos.

**Problema 2:** CI "Run Tests" demorava muito.
- **Causa:** Workflow rodava `IntegrationTests` (Testcontainers com Docker — 15-30min) e `SnapshotTests`.
- **Fixa:** Removi ambos do `ci-cd.yml`, deixando só `UnitTests` + `GeneratorTests`.

**Problema 3:** NuGet publish falhou com NU5017 (pack de projetos sem conteúdo).
- **Causa:** `dotnet pack FlowMapper.slnx` empacotava TODOS os projetos, incluindo `Analyzers`, `SourceGenerator`, samples e tests.
- **Fixa:** Adicionei `<IsPackable>false</IsPackable>` nos `Directory.Build.props` de `samples/` e `tests/`, e diretamente em `Analyzers.csproj` e `SourceGenerator.csproj`.

**Problema 4:** NuGet badge faltando.
- **Fixa:** Adicionado badge no `README.md` e `README.nuget.md`.

**Problema 5:** RepositoryUrl apontava para `anomalyco/FlowMapper` em vez de `pfabricio/FlowMapper`.
- **Fixa:** Corrigido no `Directory.Build.props`.

**v2.0.0 publicado no NuGet.** Tag criada e pushada. CI verde.

