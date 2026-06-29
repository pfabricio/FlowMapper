### 📄 ADR-0011 — Pipeline Refinement

**Status:** Accepted

**Data:** 2026-06-29

**Baseado em:** RFC-0003

----------

### Contexto

O pipeline atual do FlowMapper é:

```
Roslyn
↓
FlowPipeline.Execute()
    ├── FlowBuilder.Build()
    └── FlowValidator.Validate()
↓
FlowCodeGenerator.Generate()
```

A fase de "descoberta" (Roslyn → `MappingCandidate`) e a fase de "construção" (`FlowBuilder`) estão acopladas no `FlowPipeline.Execute()`.

O RFC propõe explicitar um estágio intermediário `MapperDefinition` entre Roslyn e FlowBuilder.

----------

### Análise

Observando o código atual:

1. `MappingCandidate` **já existe** e contém exatamente os dados que um `MapperDefinition` teria: tipos, profile, políticas, mapeamentos explícitos.
2. `MappingCandidateFactory` já faz o trabalho de "descoberta".
3. `FlowPipeline.Execute` já recebe `List<MappingCandidate>` e orquestra Build + Validate.

A separação conceitual **já existe no código**, mas não está explícita na arquitetura documentada nem no fluxo do pipeline.

----------

### Decisão

**Aceitar a separação conceitual, mas sem criar novas classes ou arquivos.**

### O que muda

1. **Renomear** `MappingCandidate` → `MapperDefinition` (oficializa o termo).
2. **Renomear** `MappingCandidateFactory` → `MapperDefinitionFactory`.
3. **Separar** `FlowPipeline.Execute` em métodos nomeados por fase:
   - `Discover(IIncrementalGeneratorInitializationContext)` → `List<MapperDefinition>`
   - `Build(MapperDefinition, FlowCache)` → `Flow`
   - `Validate(MapperDefinition, Flow)` → `List<FlowDiagnosticResult>`
   - `Generate(FlowModel)` → `string`
4. **Atualizar** a documentação do pipeline para refletir as 4 fases.

### Pipeline oficial

```
Fase 1 — Discover
    Roslyn Syntax/Semantic
    ↓
    MapperDefinitionFactory
    ↓
    List<MapperDefinition>

Fase 2 — Build
    MapperDefinition
    ↓
    FlowBuilder.Build()
    ↓
    Flow

Fase 3 — Validate
    MapperDefinition + Flow
    ↓
    FlowValidator.Validate()
    ↓
    List<FlowDiagnosticResult>

Fase 4 — Generate
    FlowModel (Flows + Diagnostics)
    ↓
    FlowCodeGenerator.Generate()
    ↓
    .g.cs
```

----------

### Benefícios

-   **Pipeline explícito**: cada fase tem entrada, processo e saída claros.
-   **Testabilidade**: cada fase testada isoladamente.
-   **Documentação alinhada**: o diagrama do pipeline reflete o código.
-   **Extensibilidade**: novas fases podem ser inseridas entre as existentes.
-   **Mudança mínima**: 0 novas classes, apenas renomeação e reorganização.

----------

### Custos

-   Renomeação quebra referências em testes e samples (mas é mecânica).
-   `FlowPipeline` existente precisará ser refatorado.

----------

### Relação com ADRs anteriores

-   **ADR-0009** (Validator Rules): a fase Validate será composta por regras independentes.
-   **ADR-0010** (Generator Decomposition): a fase Generate será composta por writers.
-   **Este ADR**: organiza as fases do pipeline, onde as decomposições dos ADRs 0009 e 0010 se encaixam.

---

