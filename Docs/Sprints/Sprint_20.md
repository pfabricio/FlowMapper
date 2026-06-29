# Sprint 20 — Pipeline Refinement (RFC-0003)

**Baseado em:** RFC-0003, ADR-0011, `Docs/Spec/rfc-0003-pipeline.md`

**Importância:** 🔵 Baixa — refatoração de nomenclatura e organização

## Objetivo

Oficializar as 4 fases do pipeline (Discover, Build, Validate, Generate) com renomeação de `MappingCandidate` → `MapperDefinition` e separação do `FlowPipeline` em métodos por fase.

---

## Tarefas

### 1. Renomear `MappingCandidate` → `MapperDefinition`

**Arquivo:** `src/FlowMapper.SourceGenerator/Models/MappingCandidate.cs` → `MapperDefinition.cs`

**O que fazer:**
1. Renomear classe `MappingCandidate` → `MapperDefinition`
2. Renomear arquivo para `MapperDefinition.cs`
3. Todas as referências a `MappingCandidate` em outros arquivos devem ser atualizadas

### 2. Renomear `MappingCandidateFactory` → `MapperDefinitionFactory`

**Arquivo:** `src/FlowMapper.SourceGenerator/MappingCandidateFactory.cs` → `MapperDefinitionFactory.cs`

**O que fazer:**
1. Renomear classe `MappingCandidateFactory` → `MapperDefinitionFactory`
2. Renomear arquivo para `MapperDefinitionFactory.cs`
3. Atualizar referências em `FlowMapperGenerator.cs`

### 3. Refatorar `FlowPipeline` em fases

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/FlowPipeline.cs`

**O que fazer:**
1. Extrair 3 métodos públicos:
   - `Build(MapperDefinition, FlowCache)` → `Flow`
   - `Validate(MapperDefinition, Flow)` → `List<FlowDiagnosticResult>`
   - `ResolveMapperName(IReadOnlyList<MapperDefinition>)` → `string`
2. `Execute()` passa a ser orquestrador que chama Build + Validate
3. Atualizar tipo de parâmetro `MappingCandidate` → `MapperDefinition`

### 4. Atualizar `FlowBuilder` e `FlowValidator`

**Arquivos:**
- `src/FlowMapper.SourceGenerator/Pipeline/Builder/FlowBuilder.cs`
- `src/FlowMapper.SourceGenerator/Pipeline/Validator/FlowValidator.cs`
- `src/FlowMapper.SourceGenerator/Pipeline/Validator/IValidationRule.cs`
- `src/FlowMapper.SourceGenerator/Pipeline/Validator/*Rule.cs`

**O que fazer:**
1. Renomear parâmetros `MappingCandidate` → `MapperDefinition` em todos os métodos
2. Sem mudança de lógica

### 5. Atualizar `FlowMapperGenerator.cs`

**Arquivo:** `src/FlowMapper.SourceGenerator/FlowMapperGenerator.cs`

**O que fazer:**
1. Atualizar referências: `MappingCandidateFactory` → `MapperDefinitionFactory`
2. Atualizar variáveis: `mapCandidates`/`profileCandidates` → `definitions`
3. Simplificar pipeline: `allCandidates` → `allDefinitions`

### 6. Atualizar testes

**Arquivos:**
- `tests/FlowMapper.UnitTests/`
- `tests/FlowMapper.Generator.Tests/`
- `tests/FlowMapper.IntegrationTests/`
- `tests/FlowMapper.SnapshotTests/`
- `tests/FlowMapper.PerformanceTests/`

**O que fazer:**
1. Substituir `MappingCandidate` → `MapperDefinition` em todos os testes
2. Atualizar snapshots (se o nome do arquivo .g.cs mudar)
3. Verificar 0 regressão

### 7. Verificar pipeline completo

**O que fazer:**
1. `dotnet build` — 0 erros, 0 warnings
2. `dotnet test` — todos passando (24/24)
3. Confirmar que o fluxo completo compila e gera código

---

## Critérios de Aceitação

- [ ] `dotnet build` — 0 erros, 0 warnings
- [ ] `dotnet test` — todos passando
- [ ] `MappingCandidate` renomeado para `MapperDefinition` em todo o código
- [ ] `MappingCandidateFactory` renomeado para `MapperDefinitionFactory`
- [ ] `FlowPipeline` tem métodos `Build()`, `Validate()`, `Execute()` públicos
- [ ] Pipeline documentado em 4 fases: Discover, Build, Validate, Generate
- [ ] Nenhuma mudança de comportamento ou output

## Referências

- `Docs/Rfc/RFC-0003 — Pipeline Refinement.md`
- `Docs/Adr/ADR-0011 — Pipeline Refinement.md`
- `Docs/Spec/rfc-0003-pipeline.md`

## Dependências

- Sprint 18 (Validator Rules) — para alinhar assinatura de `IValidationRule`

---

