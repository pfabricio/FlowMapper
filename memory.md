
## 2026-06-28 14:47:00 UTC
## Sprint 00 — Concluída

- `FlowMapper.slnx` criada com todos os projetos
- `Directory.Build.props` com configs compartilhadas
- **src/**: Abstractions, Core, SourceGenerator, Analyzers, DependencyInjection, FlowMapper (meta), Cli — todos netstandard2.0
- **tests/**: UnitTests, Generator.Tests, IntegrationTests, Benchmark.Tests, SnapshotTests
- **samples/**: BasicMapping, NestedMapping, FlattenMapping, ConstructorMapping, Profiles, DependencyInjection, Benchmark
- Dependências entre projetos configuradas
- NuGet packages: Microsoft.CodeAnalysis.CSharp 4.8, Analyzers 3.3.4, DI 8.0
- SourceGenerator configurado: EnforceExtendedAnalyzerRules, IsRoslynComponent
- **Build: 0 warnings, 0 erros**

## 2026-06-28 16:33:33 UTC
## Sprint 01 — Core Domain + Abstractions (Concluída)

- **FlowMapper.Abstractions**: MapAttribute&lt;TSource, TDestination&gt;, IMapper&lt;TSource, TDestination&gt;, FlowMapperOptions, StrictnessLevel, FlowProfileAttribute, MapPropertyAttribute
- **FlowMapper.Core**: Flow, PropertyFlow, MappingStrategy (enum), MappingPolicy, NestedFlow, ConstructorBinding, FlattenPath, FlowSignature, ProfileDefinition
- Core referencia Abstractions via csproj
- Class1.cs placeholders removidos
- **Build: 0 warnings, 0 erros**


## 2026-06-28 16:36:01 UTC
## Sprint 02 — Source Generator MVP (Concluída)

- NuGet `Microsoft.CodeAnalysis.CSharp 4.8.0` adicionado ao SourceGenerator
- `FlowMapperGenerator.cs` — IIncrementalGenerator entry point (Syntax → Semantic → Pipeline → Emit)
- `Models/MappingCandidate.cs` — resultado semântico com INamedTypeSymbol
- `Models/FlowModel.cs` — wrapper com List&lt;Flow&gt; + MapperName
- `MappingCandidateFactory.cs` — extrai tipos genéricos do MapAttribute
- `Pipeline/FlowPipeline.cs` — orquestrador
- `Pipeline/Builder/FlowBuilder.cs` — convention engine v1 (nome + tipo exatos)
- `Pipeline/Generator/FlowCodeGenerator.cs` — gera `.g.cs` com IMapper implementation
- `IsExternalInit.cs` polyfill para suporte a `init` em netstandard2.0
- Class1.cs placeholder removido
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 16:38:48 UTC
## Sprint 03 — Diagnostics Engine (Concluída)

- `Pipeline/Validator/FlowDiagnostics.cs` — DiagnosticDescriptors FM0001-FM0005
- `Pipeline/Validator/FlowDiagnosticResult.cs` — modelo de resultado (Warning/Error)
- `Pipeline/Validator/FlowValidator.cs` — valida candidato + flow: FM0001 (dest sem match), FM0002 (type mismatch), FM0003 (mapper inválido), FM0004 (source sem match)
- `FlowModel` estendido com `List&lt;FlowDiagnosticResult&gt; Diagnostics`
- `FlowPipeline.Execute` integra validação após build
- `FlowMapperGenerator.EmitSource` reporta diagnostics via `context.ReportDiagnostic`
- `NoWarn RS2008` adicionado ao csproj (release tracking não aplicável a generator)
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 16:40:26 UTC
## Sprint 04 — Nested Mapping Engine (Concluída)

- `FlowBuilder` refatorado: `IsComplexType()` (primitivo/string/collection check), `Build(ITypeSymbol, ITypeSymbol, HashSet&lt;string&gt;)` com recursão controlada via `visited` set
- Propriedades de tipo complexo com mesmo nome são detectadas e geram `NestedFlow`
- `FlowDiagnostics.FM0006` — cyclic reference detection
- `FlowCodeGenerator` atualizado: gera método `MapXxx()` privado para cada nested flow, com recursão multi-nível
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 16:43:03 UTC
## Sprint 05 — Constructor & Immutable Mapping (Concluída)

- `ConstructorResolver` — score-based algorithm: encontra construtor público com mais parâmetros compatíveis (nome + tipo) com source
- `FlowBuilder` — 2 fases: (1) detecta dest props sem public setter, (2) fallback para constructor binding
- `FlowCodeGenerator` — 3 modos: object initializer (mutável), constructor (imutável), híbrido (constructor + init)
- `FlowDiagnostics`: FM0007 (Constructor mismatch), FM0008 (Missing constructor binding)
- Records posicionais mapeados via constructor, init-only via híbrido, classes com setters via direct
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:07:57 UTC
## Sprint 06 — Flatten Mapping Engine (Concluída)

- `FlattenResolver` — DFS tree exploration de tipos complexos, coleta de leaf paths com type matching
- `FlowBuilder` — 3ª fase de resolução: para dest props não mapeadas, tenta flatten via FlattenResolver
- `FlowCodeGenerator` — gera `source.Address.City.Name` para propriedades com `Strategy.Flatten` usando `SourcePath`
- `FlowDiagnostics`: FM0009 (ambiguous path), FM0010 (path not found), FM0011 (invalid depth)
- Pipeline: Direct → Constructor → Flatten → Nested
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:10:17 UTC
## Sprint 07 — Performance & Cache Engine (Concluída)

- `Performance/FlowCache.cs` — ConcurrentDictionary thread-safe cache (sessão de compilação)
- `Performance/FlowKeyGenerator.cs` — gera chave `{SourceType}|{DestType}` via ToDisplayString()
- `Performance/SignatureGenerator.cs` — gera FlowSignature com PropertyHash (nomes + strategies ordenados)
- `FlowSignature` estendido: `PropertyHash` + `ToCacheKey()`
- `FlowBuilder.Build` — check cache antes de construir, store após build; reuso entre candidatos e nested flows
- `FlowPipeline` — cache singleton `static readonly` compartilhado entre candidatos
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:14:55 UTC
## Sprint 08 — Profile System (Concluída)

- `FlowProfileAttribute` extendido: `EnableFlatten`, `PreferConstructor`, `Strictness` (lidos pelo source generator via named args)
- `MappingCandidate` + `MappingCandidateFactory` — extraem `ProfileName` e `MappingPolicy` do `[FlowProfile]`
- `FlowBuilder.Build(candidate)` — aplica profile name e policy ao flow gerado
- `FlowCodeGenerator` — gera namespace `FlowMapper.SourceGenerator.Profiles.{ProfileName}` e adiciona `[FlowProfile]` no código gerado
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:17:01 UTC
## Sprint 09 — Fluent Configuration API (Concluída)

- `IgnoreMapAttribute` — marca propriedades a ignorar no mapeamento
- `MapPropertyAttribute` — suporte no source generator (lê e aplica mapeamentos explícitos)
- `MappingExpression<TSource, TDestination>` — API fluente com `ForMember()`, `Ignore()`, `UseConstructor()`, `DisableFlatten()`
- `ProfileDefinition.CreateMap<TSrc, TDst>()` — método factory que retorna `MappingExpression`
- `ExplicitMappingInfo` + `IgnoredProperties` em `MappingCandidate`
- `FlowBuilder.ApplyExplicitMappings()` — aplica overrides pós-convenção
- `CreateFromProfile()` em `MappingCandidateFactory` — detecta `CreateMap` em construtores de profile (para integração futura)
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:39:30 UTC
## Sprint 10 — Dependency Injection (Concluída)

- `IFlowMapper` — interface com `Map<TSource, TDestination>()` e `GetMapper<TSource, TDestination>()`
- `FlowMapperService` — implementação que resolve `IMapper<,>` do `IServiceProvider`
- `ServiceCollectionExtensions.AddFlowMapper()` — registra `IFlowMapper` e escaneia assemblies por `IMapper<,>` concretos via reflection
- `FlowMapper.DependencyInjection.csproj` — netstandard2.0, referencia Abstractions + `Microsoft.Extensions.DependencyInjection 8.0.0`
- Sample `DependencyInjection` — demonstra `AddFlowMapper()` + `IFlowMapper.Map()` com mapper manual
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:42:17 UTC
## Sprint 11 — Analyzers Layer (Concluída)

- `FlowMapper.Analyzers.csproj` — netstandard2.0, `IsRoslynComponent`, referências `Microsoft.CodeAnalysis.CSharp 4.8.0`, `Microsoft.CodeAnalysis.CSharp.Workspaces 4.8.0`, `System.Composition.AttributedModel 8.0.0`
- `DiagnosticDescriptors` — FM1001 (MapAttribute inválido), FM1002 (FlowProfile inválido), FM1003 (propriedade destino não mapeada), FM1004 (MapAttribute ausente)
- `MapAttributeAnalyzer` — valida `[Map<,>] + IMapper<,>` coexistência
- `FlowProfileAnalyzer` — valida nome não vazio, classe não estática
- `UnmappedPropertyAnalyzer` — detecta propriedades destino sem correspondente na origem
- `AddMapAttributeCodeFixProvider` — code fix que adiciona `[Map<TSrc, TDst>]` quando ausente
- **Build: 0 warnings, 0 erros | Tests: 1/1 pass**


## 2026-06-28 17:53:58 UTC
## Sprint 12 — Integration Tests (concluída)

### O que foi feito
- Implementou 5 testes de integração com `CSharpGeneratorDriver` in-memory: Basic, Constructor, Flatten, Nested, Profile
- Todos os 5 passando

### Bugs corrigidos nesta sprint
1. **`FlowMapperGenerator.cs:66`** — `Diagnostic.Create(descriptor, Location.None, d.Message)` tratava d.Message como argumento de format do messageFormat, duplicando a mensagem. Fix: criar novo `DiagnosticDescriptor` com mensagem pré-formatada.
2. **`FlowPipeline.cs:13`** — `static FlowCache` causava contaminação entre compilações de teste com símbolos de mesmo nome. Fix: cache por invocação (non-static).
3. **`FlattenResolver.cs:26`** — Match apenas pelo último segmento (`Street`) em vez do concatenado (`AddressStreet`). Fix: `string.Join("", p.Segments)`.
4. **`FlowValidator.cs:24`** — FM0002 disparava falso-positivo para nested flows. Fix: incluir `NestedFlows.Select(n => n.ParentProperty)` no `mappedDestinations`.

### Status atual
- Build: 0 erros, 0 warnings
- Testes: 8/8 passando (GeneratorTests 5/5, SnapshotTests 1/1, IntegrationTests 1/1, UnitTests 1/1)
- Projetos: Abstractions, Core, SourceGenerator, Analyzers, DI, FlowMapper meta, Cli, 4 testes, 5 samples
- Todas as src em netstandard2.0, test/sample em net10.0

### Próximo Sprint
- Sprint 13: Benchmark & Performance Tests

## 2026-06-28 18:23:58 UTC

## 2026-06-28 17:53-18:30 UTC
## Sprint 13 — Benchmark & Performance Tests (Concluída)

### O que foi feito
- `FlowMapper.PerformanceTests` criado com BenchmarkDotNet 0.14.0
- 8 benchmarks implementados em `PipelineBenchmarks.cs`:
  - Build_BasicMapping (2.889μs)
  - Build_ConstructorMapping (3.641μs)
  - Build_FlattenMapping (6.967μs)
  - Build_NestedMapping (4.451μs)
  - Pipeline_Basic (5.155μs)
  - Pipeline_FourCandidates (27.965μs)
  - Validate_Basic (4.399μs)
  - Resolve_FlattenPath (1.654μs)
- Todos 8/8 estáveis; Resolve_FlattenPath timeout-report foi falso alarme (tool timeout, benchmark roda em 1.654μs)

## 2026-06-28 15:00-18:30 UTC
## Sprint 14 — Documentation & Samples Polish (Concluída)

### O que foi feito
- **Samples**: Criados BasicMapping e FlattenMapping faltantes; implementados todos 7 samples com código real (Basic, Nested, Constructor, Flatten, Profiles, Benchmark, DependencyInjection)
- **XML docs**: Adicionados comentários XML a todas as 19 APIs públicas (Abstractions 7, Core 9, DI 3); `GenerateDocumentationFile` ativado nesses 3 projetos
- **Architecture doc**: Preenchido `Docs/Architecture/2. RUNTIME API + DEPENDENCY INJECTION.md` vazio
- **Tests substituídos**: 
  - UnitTests: 10 testes Core-domain (Flow, PropertyFlow, NestedFlow, ConstructorBinding, FlattenPath, FlowSignature, ProfileDefinition, MappingPolicy, StrictnessLevel, FlowMapperOptions)
  - SnapshotTests: 2 testes de snapshot (estrutura do código gerado para basic + constructor mapping)
  - IntegrationTests: 5 testes DI (registro, resolução, exception path)

### Bugs corrigidos
1. **IntegrationTests: FlowMapperService_Throws_When_Mapper_Not_Found** — usava `Source`/`Dest` que eram registrados pelo assembly scanning do `ManualMapper`. Fix: usar tipos `UnmappedSource`/`UnmappedDest` definidos localmente sem IMapper.

### Estado Final
- **Build**: 0 warnings, 0 erros (19 projetos)
- **Tests**: 24/24 passando (UnitTests 12/12, IntegrationTests 5/5, GeneratorTests 5/5, SnapshotTests 2/2)
- **Benchmarks**: 8/8 estáveis
- **Samples**: 7/7 compilando com código real de mapeamento
- **XML docs**: 19 tipos/membros públicos documentados; .xml gerado no build


## 2026-06-29 01:21:59 UTC
## 2026-06-28 21:20-21:50 UTC
## Sprint 15 — Profile Fluent API + Compile-time expressions (Concluída)

### O que foi feito
- **ProfileDefinition** agora é detectado automaticamente pelo Source Generator — não precisa mais de `[Map<,>]` nem `partial class`
- **`CreateMap<T1, T2>()`** no construtor do profile é lido via análise sintática (Roslyn) e gera o mapper automaticamente
- **`MapFrom(lambda)`** — expressões lambda são extraídas como string e inseridas diretamente no código gerado (ex: `s => s.Quantity * s.UnitPrice` vira `target.Total = source.Quantity * source.UnitPrice`)
- **`ForMember()`, `Ignore()`, `UseConstructor()`, `DisableFlatten()`** — todas funcionam via profile
- **`AfterMap(methodName)`** — chama método customizado pós-mapeamento (ex: cálculos)
- **`ConstructUsing(methodName)`** — usa método customizado para construir o objeto destino
- **`MapFromExpression`** novo campo em `PropertyFlow` e `ExplicitMappingInfo`
- **`IsProfileClass`** predicado no `FlowMapperGenerator` para detectar subclasses de `ProfileDefinition`
- Dois pipelines de candidatos mesclados: `[Map<,>]` tradicional + `ProfileDefinition`

### Exemplo de uso
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.FullName))
            .Ignore(d => d.Password);

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Quantity * s.UnitPrice))
            .AfterMap(nameof(CalcularFrete));
    }

    private static void CalcularFrete(Order source, OrderDto dest)
        => dest.Frete = source.Peso * 0.5m;
}
```

### Estado atual
- **Build**: 0 warnings, 0 erros (19 projetos)
- **Tests**: 24/24 passando (UnitTests 12/12, IntegrationTests 5/5, GeneratorTests 5/5, SnapshotTests 2/2)
- **Benchmarks**: 8/8 estáveis
- **Samples**: 7/7 compilando com código real de mapeamento


## 2026-06-29 13:43:41 UTC
## 2026-06-29
## Sprint 15 — Profile Fluent API + Compile-time expressions (Concluída)

- ProfileDefinition detectado automaticamente pelo SG
- MapFrom(lambda), ForMember, Ignore, UseConstructor, DisableFlatten, AfterMap, ConstructUsing
- 0 warnings, 0 erros | 24/24 testes passando | 8/8 benchmarks estáveis

## RFC Analysis & Planning — Todos os RFCs documentados

### RFC-0001 → ADR-0007 + Spec + Sprint 16
- Decisão: Remover [Map<,>] partial class, ficar só com ProfileDefinition
- Docs criados: Adr/ADR-0007, Spec/rfc-0001-api-simplification.md, Sprints/Sprint_16.md

### RFC-0002 → ADR-0008 + Spec + Sprint 17
- Decisão: AfterMap/ConstructUsing aceitam Expression<...> com lambda inline e method group
- Docs criados: Adr/ADR-0008, Spec/rfc-0002-callbacks.md, Sprints/Sprint_17.md

### RFC-0004 → ADR-0009 + Spec + Sprint 18
- Decisão: FlowValidator decomposto em regras independentes (IValidationRule)
- Docs criados: Adr/ADR-0009, Spec/rfc-0004-validator-rules.md, Sprints/Sprint_18.md

### RFC-0005 → ADR-0010 + Spec + Sprint 19
- Decisão: FlowCodeGenerator decomposto em writers (ICodeWriter)
- Docs criados: Adr/ADR-0010, Spec/rfc-0005-generator-writers.md, Sprints/Sprint_19.md

### RFC-0003 → ADR-0011 + Spec + Sprint 20
- Decisão: Pipeline em 4 fases (Discover, Build, Validate, Generate), MappingCandidate → MapperDefinition
- Docs criados: Adr/ADR-0011, Spec/rfc-0003-pipeline.md, Sprints/Sprint_20.md

### Total: 5 ADRs, 5 Specs, 5 Sprints criados


## 2026-06-29 13:51:39 UTC
## 2026-06-29 — ADR-0007 revisado: FlowMapper Profile-First

- Decisão alterada: `[Map<,>]` não é removido, mas escondido do usuário
- `ProfileDefinition` = única API pública documentada
- `[Map<,>]` ganha `[EditorBrowsable(Never)]` + `[Obsolete]` — pipeline interno mantido
- Samples migrados para 100% ProfileDefinition
- Zero breaking change

