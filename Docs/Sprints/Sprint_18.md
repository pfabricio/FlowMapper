# Sprint 18 — Validator Rules Decomposition (RFC-0004)

**Baseado em:** RFC-0004, ADR-0009, `Docs/Spec/rfc-0004-validator-rules.md`

**Importância:** 🟡 Média — melhoria de qualidade interna

## Objetivo

Decompor o `FlowValidator` monolítico em regras independentes, cada uma com interface `IValidationRule`, testáveis isoladamente.

---

## Tarefas

### 1. Criar `IValidationRule` interface

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/IValidationRule.cs`

```csharp
namespace FlowMapper.SourceGenerator.Pipeline.Validator;

public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow);
}
```

### 2. Extrair `PropertyMatchRule`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/PropertyMatchRule.cs`

**Conteúdo:** FM0001, FM0002, FM0004 — migrar lógica das linhas 32-66 do `FlowValidator` atual.

**Específico:**
- `mappedDestinations` inclui `flow.NestedFlows.Select(n => n.ParentProperty)` (já implementado)
- `mappedSources` inclui apenas `flow.Properties.Select(p => p.SourceProperty)`

### 3. Extrair `InvalidMapperRule`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/InvalidMapperRule.cs`

**Conteúdo:** FM0003 — migrar lógica das linhas 68-76 do `FlowValidator` atual.

### 4. Criar `ConstructorRule`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/ConstructorRule.cs`

**Conteúdo:** FM0007, FM0008 — validar `flow.ConstructorBinding`.

> ⚠️ Nota: hoje FM0007/FM0008 são emitidos pelo FlowBuilder. Esta sprint deve **mover** a emissão para o Validator, removendo do Builder.

### 5. Criar `CycleRule`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/CycleRule.cs`

**Conteúdo:** FM0006 — detectar ciclos em `flow.NestedFlows`.

### 6. Criar `FlattenRule`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/FlattenRule.cs`

**Conteúdo:** FM0009, FM0010, FM0011 — validar `prop.FlattenPath` para propriedades com `Strategy.Flatten`.

### 7. Refatorar `FlowValidator`

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Validator/FlowValidator.cs`

**Mudança:** Remover toda a lógica inline, substituir por iteração das regras:

```csharp
public static List<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow)
{
    var diagnostics = new List<FlowDiagnosticResult>();
    foreach (var rule in Rules)
        diagnostics.AddRange(rule.Validate(candidate, flow));
    return diagnostics;
}
```

### 8. Atualizar testes

**Arquivos:**
- `tests/FlowMapper.UnitTests/` — adicionar testes por regra
- `tests/FlowMapper.PerformanceTests/PipelineBenchmarks.cs` — ajustar chamada (assinatura não muda)

### 9. Verificar integração

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/FlowPipeline.cs`

**Verificar:** chamada `FlowValidator.Validate(candidate, flow)` continua funcionando (assinatura inalterada).

---

## Critérios de Aceitação

- [ ] `dotnet build` — 0 erros, 0 warnings
- [ ] `dotnet test` — todos passando
- [ ] `IValidationRule` interface criada e implementada por 5 regras
- [ ] `FlowValidator.Validate()` delega para as regras
- [ ] Nenhuma lógica de validação inline restou em `FlowValidator`
- [ ] FM0007/FM0008 movidos do FlowBuilder para ConstructorRule
- [ ] Cada regra testável isoladamente

## Referências

- `Docs/Rfc/RFC-0004 — Validator Rules.md`
- `Docs/Adr/ADR-0009 — Validator Rules.md`
- `Docs/Spec/rfc-0004-validator-rules.md`

## Dependências

- Nenhuma — pode ser executada em paralelo com Sprint 16 e 17

---

