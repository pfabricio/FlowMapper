### 📘 Spec — RFC-0004: Validator Rules Decomposition

**Versão:** 1.0
**Status:** Aprovado (ADR-0009)
**Baseado em:** RFC-0004, ADR-0009

----------

### 1. Objetivo

Decompor o `FlowValidator` monolítico em regras independentes, cada uma com responsabilidade única e testável isoladamente.

----------

### 2. Interface

```csharp
// Pipeline/Validator/IValidationRule.cs
namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow);
}
```

----------

### 3. Regras

### 3.1 `PropertyMatchRule` — FM0001, FM0002, FM0004

**Extraído de:** FlowValidator.Validate() linhas 32-66

**Responsabilidade:** Validar que propriedades source e destination têm correspondência.

| Código | Severidade | Mensagem |
|--------|------------|----------|
| FM0001 | Warning | Destination property '{prop}' is not mapped |
| FM0002 | Error | Cannot map '{sourceType}' to '{destType}' for property '{prop}' |
| FM0004 | Warning | Source property '{prop}' has no matching destination |

```csharp
public class PropertyMatchRule : IValidationRule
{
    public string RuleId => "PropertyMatch";

    public IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        var sourceProps = candidate.SourceType
            .GetMembers().OfType<IPropertySymbol>().ToList();
        var destProps = candidate.DestinationType
            .GetMembers().OfType<IPropertySymbol>().ToList();

        var mappedDestinations = new HashSet<string>(
            flow.Properties.Select(p => p.DestinationProperty)
                .Concat(flow.NestedFlows.Select(n => n.ParentProperty)));

        var mappedSources = new HashSet<string>(
            flow.Properties.Select(p => p.SourceProperty));

        // FM0004 — Source sem match
        foreach (var sp in sourceProps)
            if (!mappedSources.Contains(sp.Name))
                yield return FlowDiagnosticResult.Warning("FM0004",
                    $"Source property '{sp.Name}' has no matching destination");

        // FM0001 — Dest sem match
        // FM0002 — Type mismatch
        foreach (var dp in destProps)
        {
            if (mappedDestinations.Contains(dp.Name)) continue;
            var sourceMatch = sourceProps.FirstOrDefault(s => s.Name == dp.Name);
            if (sourceMatch != null)
            {
                if (!SymbolEqualityComparer.Default.Equals(sourceMatch.Type, dp.Type))
                    yield return FlowDiagnosticResult.Error("FM0002",
                        $"Cannot map '{sourceMatch.Type.Name}' to '{dp.Type.Name}' for property '{dp.Name}'");
            }
            else
            {
                yield return FlowDiagnosticResult.Warning("FM0001",
                    $"Destination property '{dp.Name}' is not mapped");
            }
        }
    }
}
```

### 3.2 `InvalidMapperRule` — FM0003

**Extraído de:** FlowValidator.Validate() linhas 68-76

**Responsabilidade:** Detectar mappers que não mapeiam nenhuma propriedade.

| Código | Severidade | Mensagem |
|--------|------------|----------|
| FM0003 | Error | Mapper '{name}' is invalid or incomplete |

```csharp
public class InvalidMapperRule : IValidationRule
{
    public string RuleId => "InvalidMapper";

    public IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        var sourceProps = candidate.SourceType
            .GetMembers().OfType<IPropertySymbol>().ToList();

        if (flow.Properties.Count == 0 && sourceProps.Count > 0)
        {
            yield return FlowDiagnosticResult.Error("FM0003",
                $"Mapper '{candidate.MapperType.Name}' is invalid or incomplete");
        }
    }
}
```

### 3.3 `ConstructorRule` — FM0007, FM0008

**Responsabilidade:** Validar construtores e bindings de parâmetros.

> ⚠️ Atualmente FM0007/FM0008 são emitidos pelo FlowBuilder, não pelo FlowValidator. Esta regra consolidará a emissão no Validator.

| Código | Severidade | Mensagem |
|--------|------------|----------|
| FM0007 | Warning | No suitable constructor found for type '{type}' |
| FM0008 | Error | Required constructor parameter '{param}' not mapped |

```csharp
public class ConstructorRule : IValidationRule
{
    public string RuleId => "Constructor";

    public IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        if (flow.ConstructorBinding == null) yield break;

        // FM0007 — Nenhum construtor compatível
        if (!flow.ConstructorBinding.IsValid)
            yield return FlowDiagnosticResult.Warning("FM0007",
                $"No suitable constructor found for type '{candidate.DestinationType.Name}'");

        // FM0008 — Parâmetro obrigatório não mapeado
        foreach (var param in flow.ConstructorBinding.Parameters)
        {
            if (!param.IsMapped)
                yield return FlowDiagnosticResult.Error("FM0008",
                    $"Required constructor parameter '{param.Name}' not mapped");
        }
    }
}
```

### 3.4 `CycleRule` — FM0006

**Responsabilidade:** Detectar referências cíclicas no grafo de mapeamento.

| Código | Severidade | Mensagem |
|--------|------------|----------|
| FM0006 | Error | Cycle detected in mapping path: {path} |

```csharp
public class CycleRule : IValidationRule
{
    public string RuleId => "Cycle";

    public IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        // FM0006 — Verificar ciclos no grafo
        var visited = new HashSet<string>();
        foreach (var nested in flow.NestedFlows)
        {
            if (!visited.Add($"{nested.SourceType}|{nested.DestinationType}"))
            {
                yield return FlowDiagnosticResult.Error("FM0006",
                    $"Cycle detected in mapping path: {nested.SourceType.Name} → {nested.DestinationType.Name}");
            }
        }
    }
}
```

### 3.5 `FlattenRule` — FM0009, FM0010, FM0011

**Responsabilidade:** Validar caminhos flatten.

| Código | Severidade | Mensagem |
|--------|------------|----------|
| FM0009 | Error | Multiple paths found for property '{prop}' |
| FM0010 | Warning | No valid path found for '{prop}' |
| FM0011 | Error | Cycle or invalid depth detected in flatten graph |

```csharp
public class FlattenRule : IValidationRule
{
    public string RuleId => "Flatten";

    public IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        foreach (var prop in flow.Properties.Where(p => p.Strategy == MappingStrategy.Flatten))
        {
            if (prop.FlattenPath == null)
            {
                yield return FlowDiagnosticResult.Warning("FM0010",
                    $"No valid path found for '{prop.DestinationProperty}'");
                continue;
            }

            if (prop.FlattenPath.IsAmbiguous)
            {
                yield return FlowDiagnosticResult.Error("FM0009",
                    $"Multiple paths found for property '{prop.DestinationProperty}'");
            }

            if (prop.FlattenPath.Segments.Count > 5) // profundidade arbitrária
            {
                yield return FlowDiagnosticResult.Error("FM0011",
                    $"Cycle or invalid depth detected in flatten graph");
            }
        }
    }
}
```

----------

### 4. Orquestrador

```csharp
// Pipeline/Validator/FlowValidator.cs — REFATORADO
public static class FlowValidator
{
    private static readonly List<IValidationRule> Rules = new()
    {
        new PropertyMatchRule(),
        new InvalidMapperRule(),
        new ConstructorRule(),
        new CycleRule(),
        new FlattenRule()
    };

    public static List<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
    {
        var diagnostics = new List<FlowDiagnosticResult>();
        foreach (var rule in Rules)
        {
            diagnostics.AddRange(rule.Validate(candidate, flow));
        }
        return diagnostics;
    }
}
```

----------

### 5. Testes

| Teste | Regra |
|-------|-------|
| `PropertyMatchRule_EmitsFM0001_WhenDestPropertyMissing` | PropertyMatchRule |
| `PropertyMatchRule_EmitsFM0002_WhenTypeMismatch` | PropertyMatchRule |
| `PropertyMatchRule_EmitsFM0004_WhenSourcePropertyMissing` | PropertyMatchRule |
| `InvalidMapperRule_EmitsFM0003_WhenNoPropertiesMapped` | InvalidMapperRule |
| `ConstructorRule_EmitsFM0007_WhenNoSuitableConstructor` | ConstructorRule |
| `ConstructorRule_EmitsFM0008_WhenParameterNotMapped` | ConstructorRule |
| `CycleRule_EmitsFM0006_WhenNestedCycleDetected` | CycleRule |
| `FlattenRule_EmitsFM0009_WhenAmbiguousPath` | FlattenRule |
| `FlattenRule_EmitsFM0010_WhenPathNotFound` | FlattenRule |
| `FlowValidator_AggregatesAllRules` | Orquestrador |

----------

### 6. O que muda em cada arquivo

| Arquivo | Ação |
|---------|------|
| `Pipeline/Validator/FlowValidator.cs` | Refatorar: remover lógica inline, iterar regras |
| `Pipeline/Validator/IValidationRule.cs` | **Criar**: interface |
| `Pipeline/Validator/PropertyMatchRule.cs` | **Criar**: FM0001, FM0002, FM0004 |
| `Pipeline/Validator/InvalidMapperRule.cs` | **Criar**: FM0003 |
| `Pipeline/Validator/ConstructorRule.cs` | **Criar**: FM0007, FM0008 |
| `Pipeline/Validator/CycleRule.cs` | **Criar**: FM0006 |
| `Pipeline/Validator/FlattenRule.cs` | **Criar**: FM0009, FM0010, FM0011 |
| `FlowPipeline.cs` | Nenhuma (chamada permanece igual: `FlowValidator.Validate()`) |

---

