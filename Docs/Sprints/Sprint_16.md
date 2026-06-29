# Sprint 16 — FlowMapper Profile-First (RFC-0001)

**Baseado em:** RFC-0001, ADR-0007, `Docs/Spec/rfc-0001-api-simplification.md`

**Importância:** 🔴 Alta — impacto direto na API pública antes da v1.0

## Objetivo

Tornar `ProfileDefinition` a única API pública, escondendo `[Map<,>]` do usuário sem quebrar compatibilidade nem remover o pipeline interno.

---

## Tarefas

### 1. Abstractions — Esconder `MapAttribute<,>`

**Arquivo:** `src/FlowMapper.Abstractions/MapAttribute.cs`

**O que fazer:**
1. Adicionar `[EditorBrowsable(EditorBrowsableState.Never)]` — some do IntelliSense
2. Adicionar `[Obsolete("Use ProfileDefinition.CreateMap<T1, T2>() instead")]` — warning para quem usa
3. Manter o atributo existindo (backward compat)

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use ProfileDefinition.CreateMap<T1, T2>() instead. " +
          "See https://flowmapper.dev/docs/migration-v1")]
public class MapAttribute<TSource, TDestination> : Attribute { }
```

### 2. Source Generator — Pipeline de `[Map<,>]` permanece

**Arquivos:**
- `src/FlowMapper.SourceGenerator/FlowMapperGenerator.cs`
- `src/FlowMapper.SourceGenerator/MappingCandidateFactory.cs`
- `src/FlowMapper.SourceGenerator/Models/MappingCandidate.cs`

**O que fazer:**
1. **Nada.** O pipeline de `[Map<,>]` continua existindo e funcionando.
2. Verificar que `CreateFromAttribute()` + `CreateFromProfile()` coexistem sem conflito.
3. Garantir que o merge dos dois pipelines em `FlowMapperGenerator` continua correto.

### 3. Analyzers — Atualizar ou remover `MapAttributeAnalyzer`

**Arquivos:**
- `src/FlowMapper.Analyzers/MapAttributeAnalyzer.cs`
- `src/FlowMapper.Analyzers/AddMapAttributeCodeFixProvider.cs`

**O que fazer:**
1. `MapAttributeAnalyzer`: remover ou simplificar (o `[Obsolete]` já cobre)
2. `AddMapAttributeCodeFixProvider`: **remover** (ninguém deve adicionar `[Map<,>]` novo)

### 4. Criar `MigrateToProfileCodeFixProvider`

**Arquivo:** `src/FlowMapper.Analyzers/MigrateToProfileCodeFixProvider.cs`

**O que fazer:**
Criar code fix que converte:
```csharp
[Map<User, UserDto>]
public partial class UserMapper;
```
para:
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

### 5. Samples — Migrar todos para `ProfileDefinition`

**Arquivos:**
- `samples/BasicMapping/`
- `samples/NestedMapping/`
- `samples/FlattenMapping/`
- `samples/ConstructorMapping/`
- `samples/DependencyInjection/`
- `samples/Benchmark/`

**O que fazer:**
1. Substituir `[Map<,>] partial class` por `ProfileDefinition` com `CreateMap`
2. Nomear o profile de acordo com o domínio (ex: `BasicMappingProfile`, `NestedMappingProfile`)
3. Verificar que todos compilam e rodam

### 6. Testes — Manter testes de `[Map<,>]`

**Arquivos:**
- `tests/FlowMapper.Generator.Tests/`
- `tests/FlowMapper.IntegrationTests/`
- `tests/FlowMapper.SnapshotTests/`
- `tests/FlowMapper.UnitTests/`

**O que fazer:**
1. **Manter** testes com `[Map<,>]` (backward compat deve continuar)
2. **Adicionar** testes com `ProfileDefinition` (novo fluxo oficial)
3. Atualizar snapshots se necessário
4. Garantir 0 perda de cobertura

### 7. Build & Regressão

**O que fazer:**
1. `dotnet build` sem erros
2. `dotnet test` — todos passando
3. Verificar que `[Map<,>]` ainda funciona (backward compat)
4. Verificar que `ProfileDefinition` funciona (novo fluxo)

---

## Critérios de Aceitação

- [ ] `dotnet build` — 0 erros, 0 warnings (excluindo `[Obsolete]` intencional)
- [ ] `dotnet test` — todos passando
- [ ] `MapAttribute<,>` marcado com `[EditorBrowsable(Never)]` + `[Obsolete]`
- [ ] Nenhum sample usa `[Map<,>] partial class`
- [ ] Pipeline de `[Map<,>]` no SG continua intacto
- [ ] Code fix provider `MigrateToProfileCodeFixProvider` criado
- [ ] `AddMapAttributeCodeFixProvider` removido
- [ ] Backward compat mantida (testes com `[Map<,>]` passam)

## Referências

- `Docs/Rfc/RFC-0001 — Public API Simplification.md`
- `Docs/Adr/ADR-0007 — Public API Simplification.md`
- `Docs/Spec/rfc-0001-api-simplification.md`

## Dependências

- Sprints 01-15 — features base implementadas e estáveis

---

