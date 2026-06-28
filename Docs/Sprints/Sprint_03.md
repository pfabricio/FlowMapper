# Sprint 03 — Diagnostics Engine (Camada 1)

**Baseado em:** `Docs/Engines/3. DIAGNOSTICS ENGINE.md`, `Docs/Spec/SPEC.md` (seção 5.7)

## Objetivo

Implementar a camada de diagnóstico do Source Generator — detectar problemas de mapeamento em build-time e reportar como warnings/errors do Roslyn.

---

## Tarefas

### 1. `FlowDiagnostics` — DiagnosticDescriptors
```csharp
public static class FlowDiagnostics
{
    public static readonly DiagnosticDescriptor MissingDestinationProperty = new(
        id: "FM0001",
        title: "Property not mapped",
        messageFormat: "Destination property '{0}' is not mapped",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor TypeMismatch = new(
        id: "FM0002",
        title: "Type mismatch",
        messageFormat: "Cannot map '{0}' to '{1}'",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor InvalidMapper = new(
        id: "FM0003",
        title: "Invalid mapper",
        messageFormat: "Mapper '{0}' is invalid or incomplete",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompleteMapping = new(
        id: "FM0004",
        title: "Incomplete mapping",
        messageFormat: "Source property '{0}' has no matching destination",
        category: "FlowMapper",
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor MalformedMapAttribute = new(
        id: "FM0005",
        title: "Malformed Map attribute",
        messageFormat: "MapAttribute is malformed on '{0}'",
        category: "FlowMapper",
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
```

### 2. `FlowDiagnosticResult`
```csharp
public class FlowDiagnosticResult
{
    public string Id { get; set; }
    public string Message { get; set; }
    public bool IsWarning { get; set; }
    public Location? Location { get; set; }

    public static FlowDiagnosticResult Warning(string id, string message)
        => new() { Id = id, Message = message, IsWarning = true };

    public static FlowDiagnosticResult Error(string id, string message)
        => new() { Id = id, Message = message, IsWarning = false };
}
```

### 3. `FlowValidator.Validate`
```csharp
public static List<FlowDiagnosticResult> Validate(Flow flow)
```

Validações v1:
- **FM0001** — Propriedade de destino sem mapeamento correspondente
- **FM0002** — Tipo incompatível entre source e destination
- **FM0003** — Mapper inválido (estrutura incorreta)
- **FM0004** — Propriedade de origem sem match no destino
- **FM0005** — Atributo Map mal formado

### 4. Integração no Generator
No `EmitSource` do `FlowMapperGenerator`:
```csharp
private static void EmitSource(SourceProductionContext context, FlowModel model)
{
    foreach (var flow in model.Flows)
    {
        var diagnostics = FlowValidator.Validate(flow);
        foreach (var d in diagnostics)
        {
            var descriptor = d.IsWarning
                ? FlowDiagnostics.MissingDestinationProperty
                : FlowDiagnostics.TypeMismatch;
            context.ReportDiagnostic(
                Diagnostic.Create(descriptor, Location.None, d.Message));
        }
        var code = FlowCodeGenerator.Generate(flow);
        context.AddSource($"{flow.SourceType}Mapper.g.cs", code);
    }
}
```

### 5. Regras de Severidade
| Situação | Severidade |
|---|---|
| Propriedade não mapeada | Warning |
| Tipo incompatível | Error |
| Mapper inválido | Error |
| Atributo mal formado | Error |
| Propriedade extra na origem | Warning |

### 6. Testes
- Mapper com propriedade não mapeada → warning FM0001
- Atributo sem tipos genéricos → error FM0005
- Flow vazio → warning FM0001 para cada propriedade de destino

## Critérios de Aceitação

- [ ] Warnings FM0001 e FM0004 aparecem no Error List do VS
- [ ] Errors FM0002, FM0003, FM0005 quebram o build
- [ ] Mensagens de diagnóstico são claras e curtas
- [ ] Integração com `SourceProductionContext.ReportDiagnostic`
- [ ] Código ainda é gerado mesmo com warnings (não bloqueante)

## Referências

- `Docs/Engines/3. DIAGNOSTICS ENGINE.md` — arquitetura completa, FlowValidator, FlowDiagnostics
- `Docs/Spec/SPEC.md` seção 5.7 — tabela de diagnósticos
- `Docs/Architecture/Estrutura-Soluction.md` — namespace `Validation/` e `Diagnostics/`

## Dependências

- Sprint 02 — pipeline do Source Generator funcionando
