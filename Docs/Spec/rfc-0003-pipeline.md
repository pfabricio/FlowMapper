### 📘 Spec — RFC-0003: Pipeline Refinement

**Versão:** 1.0
**Status:** Aprovado (ADR-0011)
**Baseado em:** RFC-0003, ADR-0011

----------

### 1. Objetivo

Oficializar as 4 fases do pipeline (Discover, Build, Validate, Generate) com renomeação de `MappingCandidate` → `MapperDefinition` e separação do `FlowPipeline` em métodos por fase.

----------

### 2. Renomeações

| Antes | Depois | Onde |
|-------|--------|------|
| `MappingCandidate` | `MapperDefinition` | `Models/MapperDefinition.cs` |
| `MappingCandidateFactory` | `MapperDefinitionFactory` | `MapperDefinitionFactory.cs` |
| `ExplicitMappingInfo` | (mantém, interno) | `MapperDefinition.cs` |

**Arquivos afetados:**
- `src/FlowMapper.SourceGenerator/Models/MappingCandidate.cs` → renomear classe
- `src/FlowMapper.SourceGenerator/MappingCandidateFactory.cs` → renomear classe
- `src/FlowMapper.SourceGenerator/FlowMapperGenerator.cs` → atualizar referências
- `src/FlowMapper.SourceGenerator/Pipeline/FlowPipeline.cs` → atualizar
- `src/FlowMapper.SourceGenerator/Pipeline/Builder/FlowBuilder.cs` → atualizar
- `src/FlowMapper.SourceGenerator/Pipeline/Validator/FlowValidator.cs` → atualizar (assinatura)
- `src/FlowMapper.SourceGenerator/Pipeline/Validator/IValidationRule.cs` → atualizar

----------

### 3. Pipeline em 4 Fases

### 3.1 Fase Discover

**Entrada:** Roslyn `IncrementalGeneratorInitializationContext`
**Processo:** `MapperDefinitionFactory.Create()` / `CreateFromProfile()`
**Saída:** `List<MapperDefinition>`

```csharp
// FlowMapperGenerator.cs
var definitions = context.SyntaxProvider
    .CreateSyntaxProvider(predicate: IsProfileClass, transform: GetDefinitions)
    .Where(x => x is not null)
    .Select((x, _) => x!);
```

### 3.2 Fase Build

**Entrada:** `MapperDefinition`
**Processo:** `FlowBuilder.Build(definition, cache)`
**Saída:** `Flow`

```csharp
// FlowPipeline.cs
Flow Build(MapperDefinition definition, FlowCache cache)
    => FlowBuilder.Build(definition, cache);
```

### 3.3 Fase Validate

**Entrada:** `MapperDefinition + Flow`
**Processo:** `FlowValidator.Validate(definition, flow)`
**Saída:** `List<FlowDiagnosticResult>`

```csharp
// FlowPipeline.cs
List<FlowDiagnosticResult> Validate(MapperDefinition definition, Flow flow)
    => FlowValidator.Validate(definition, flow);
```

### 3.4 Fase Generate

**Entrada:** `FlowModel`
**Processo:** `FlowCodeGenerator.Generate(model)`
**Saída:** `string (.g.cs)`

```csharp
// FlowMapperGenerator.cs
var code = FlowCodeGenerator.Generate(model);
context.AddSource($"{model.MapperName}.g.cs", code);
```

----------

### 4. FlowPipeline Refatorado

```csharp
public static class FlowPipeline
{
    public static FlowModel Execute(IReadOnlyList<MapperDefinition> definitions)
    {
        var cache = new FlowCache();
        var flows = new List<Flow>();
        var allDiagnostics = new List<FlowDiagnosticResult>();
        var mapperName = ResolveMapperName(definitions);

        foreach (var definition in definitions)
        {
            var flow = Build(definition, cache);
            var diagnostics = Validate(definition, flow);
            flows.Add(flow);
            allDiagnostics.AddRange(diagnostics);
        }

        return new FlowModel(flows, mapperName, allDiagnostics);
    }

    public static Flow Build(MapperDefinition definition, FlowCache cache)
        => FlowBuilder.Build(definition, cache);

    public static List<FlowDiagnosticResult> Validate(MapperDefinition definition, Flow flow)
        => FlowValidator.Validate(definition, flow);

    private static string ResolveMapperName(IReadOnlyList<MapperDefinition> definitions)
    {
        string? name = null;
        foreach (var def in definitions)
        {
            var n = def.MapperName ?? def.SourceType.Name + "To" + def.DestinationType.Name + "Mapper";
            if (name == null)
                name = n;
            else if (name != n)
                return "AggregateMapper";
        }
        return name ?? "UnknownMapper";
    }
}
```

> A assinatura de `FlowBuilder.Build()` muda de `Build(MappingCandidate, FlowCache)` para `Build(MapperDefinition, FlowCache)` (apenas renomeação do tipo).

> A assinatura de `FlowValidator.Validate()` muda de `Validate(MappingCandidate, Flow)` para `Validate(MapperDefinition, Flow)` (apenas renomeação do tipo).

----------

### 5. O que muda

| Arquivo | Ação |
|---------|------|
| `Models/MappingCandidate.cs` | Renomear `MappingCandidate` → `MapperDefinition`, `ExplicitMappingInfo` mantém |
| `MappingCandidateFactory.cs` | Renomear classe → `MapperDefinitionFactory` |
| `FlowMapperGenerator.cs` | Atualizar referências de tipo |
| `FlowPipeline.cs` | Refatorar em fases + renomear tipo |
| `FlowBuilder.cs` | Renomear parâmetro de tipo |
| `FlowValidator.cs` | Renomear parâmetro de tipo |
| `IValidationRule.cs` | Renomear parâmetro de tipo |
| Demais regras (`*Rule.cs`) | Renomear parâmetro de tipo |
| Testes | Renomear referências |

----------

### 6. Testes

| Teste | Fase |
|-------|------|
| `FlowPipeline_Discover_ReturnsDefinitions` | Discover |
| `FlowPipeline_Build_ReturnsFlow` | Build |
| `FlowPipeline_Validate_ReturnsDiagnostics` | Validate |
| `FlowPipeline_Execute_AggregatesAllPhases` | Pipeline completo |
| `FlowPipeline_ResolveMapperName_MergesCorrectly` | Utilitário |

----------

### 7. Pipeline Visual

```
┌─────────────────────────────────────┐
│  Fase 1 — DISCOVER                  │
│  Roslyn Syntax → MapperDefinition   │
│  MapperDefinitionFactory.Create()   │
└──────────────────┬──────────────────┘
                   ↓
┌─────────────────────────────────────┐
│  Fase 2 — BUILD                     │
│  MapperDefinition → Flow            │
│  FlowBuilder.Build(definition)      │
└──────────────────┬──────────────────┘
                   ↓
┌─────────────────────────────────────┐
│  Fase 3 — VALIDATE                  │
│  MapperDefinition + Flow → Diags    │
│  FlowValidator.Validate(def, flow)  │
└──────────────────┬──────────────────┘
                   ↓
┌─────────────────────────────────────┐
│  Fase 4 — GENERATE                  │
│  FlowModel → .g.cs                  │
│  FlowCodeGenerator.Generate(model)  │
└─────────────────────────────────────┘
```

---

