### 📄 ADR-0009 — Validator Rules Decomposition

**Status:** Accepted

**Data:** 2026-06-29

**Baseado em:** RFC-0004

----------

### Contexto

O `FlowValidator` atualmente é uma **classe estática monolítica** com um único método `Validate()` que:

1. Itera propriedades source
2. Itera propriedades destination
3. Verifica type mismatches
4. Verifica propriedades não mapeadas
5. Verifica mapper inválido

Tudo no mesmo método, num único arquivo de ~78 linhas.

Problemas:
-   **Dificuldade de teste**: testar uma validação específica requer setup de todas as outras.
-   **Acoplamento**: regras de constructor, flatten e ciclo estão espalhadas entre FlowBuilder e FlowValidator.
-   **Extensibilidade**: adicionar uma nova regra significa editar o método `Validate()`.
-   **Legibilidade**: responsabilidades misturadas dificultam entender o que cada regra faz.

----------

### Decisão

**Decompor o `FlowValidator` em regras independentes, cada uma implementando uma interface comum.**

### Estrutura

```
Pipeline/Validator/
├── IValidationRule.cs          ← interface
├── PropertyMatchRule.cs        ← FM0001, FM0002, FM0004
├── InvalidMapperRule.cs        ← FM0003
├── ConstructorRule.cs          ← FM0007, FM0008
├── CycleRule.cs                ← FM0006
├── FlattenRule.cs              ← FM0009, FM0010, FM0011
└── FlowValidator.cs            ← orquestrador (itera regras)
```

### Interface

```csharp
public interface IValidationRule
{
    string RuleId { get; }
    IEnumerable<FlowDiagnosticResult> Validate(MappingCandidate candidate, Flow flow);
}
```

----------

### Benefícios

-   **Testabilidade**: cada regra é testada isoladamente com setup mínimo.
-   **Extensibilidade**: nova regra = nova classe + registrar no orquestrador.
-   **Legibilidade**: cada arquivo tem uma responsabilidade única.
-   **Manutenção**: alterar uma regra não afeta as outras.
-   **Alinhamento com RFC-0005** (Generator Decomposition): mesma filosofia de decomposição.

----------

### Custos

-   Mais arquivos no projeto (7 vs 1).
-   Orquestrador adicional (`FlowValidator` atualizado para iterar regras).
-   Refatoração de testes existentes.

----------

### Regras

| Regra | Códigos | Responsabilidade |
|-------|---------|------------------|
| `PropertyMatchRule` | FM0001, FM0002, FM0004 | Propriedades source/dest sem match, type mismatch |
| `InvalidMapperRule` | FM0003 | Mapper sem propriedades mapeadas |
| `ConstructorRule` | FM0007, FM0008 | Construtor não encontrado, binding ausente |
| `CycleRule` | FM0006 | Referência cíclica no grafo de mapeamento |
| `FlattenRule` | FM0009, FM0010, FM0011 | Caminho ambíguo, não encontrado, profundidade inválida |

---

