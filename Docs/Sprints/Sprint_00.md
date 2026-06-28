# Sprint 00 — Setup da Solução

**Baseado em:** `Docs/Architecture/Estrutura-Soluction.md`, `Docs/Spec/SPEC.md`

## Objetivo

Criar a estrutura completa da solução FlowMapper com todos os projetos definidos na arquitetura, configurar dependências compartilhadas e garantir que o ambiente de build esteja funcional.

## Tarefas

### 1. Criar `FlowMapper.sln`
- Criar solution no Visual Studio / `dotnet new sln`
- Nome: `FlowMapper.sln`

### 2. Criar `Directory.Build.props`
- Propriedades compartilhadas:
  - `LangVersion = 12`
  - `ImplicitUsings = enable`
  - `Nullable = enable`
  - `TreatWarningsAsErrors = true`
  - Versionamento semântico (`1.0.0`)

### 3. Scaffold dos Projetos `src/`

| Projeto | Template | Descrição |
|---|---|---|
| `src/FlowMapper.Abstractions` | `classlib` | Atributos, interfaces, options (sem Roslyn) |
| `src/FlowMapper.Core` | `classlib` | Domínio: Flow, PropertyFlow, Policy |
| `src/FlowMapper.SourceGenerator` | `classlib` | Roslyn Incremental Generator + pipelines |
| `src/FlowMapper.Analyzers` | `classlib` | Analyzer IDE-level (v1 opcional) |
| `src/FlowMapper.DependencyInjection` | `classlib` | Extensão `AddFlowMapper()` |
| `src/FlowMapper` | `classlib` | Meta-package de entrada do usuário |
| `src/FlowMapper.Cli` | `console` | CLI tool (dotnet flowmapper) |

### 4. Scaffold dos Projetos `tests/`

| Projeto | Template |
|---|---|
| `tests/FlowMapper.UnitTests` | `xunit` |
| `tests/FlowMapper.Generator.Tests` | `xunit` |
| `tests/FlowMapper.IntegrationTests` | `xunit` |
| `tests/FlowMapper.Benchmark.Tests` | `console` (BenchmarkDotNet) |
| `tests/FlowMapper.SnapshotTests` | `xunit` |

### 5. Scaffold dos Projetos `samples/`

| Projeto | Template |
|---|---|
| `samples/BasicMapping` | `console` |
| `samples/NestedMapping` | `console` |
| `samples/FlattenMapping` | `console` |
| `samples/ConstructorMapping` | `console` |
| `samples/Profiles` | `console` |
| `samples/DependencyInjection` | `console` |
| `samples/Benchmark` | `console` |

### 6. Adicionar Projetos à Solution
```bash
dotnet sln FlowMapper.sln add src/**/*.csproj
dotnet sln FlowMapper.sln add tests/**/*.csproj
dotnet sln FlowMapper.sln add samples/**/*.csproj
```

### 7. Configurar Dependências entre Projetos

| Projeto | Referencia |
|---|---|
| `FlowMapper.SourceGenerator` | `FlowMapper.Abstractions`, `FlowMapper.Core` |
| `FlowMapper.DependencyInjection` | `FlowMapper.Abstractions` |
| `FlowMapper` | `FlowMapper.Abstractions`, `FlowMapper.SourceGenerator`, `FlowMapper.DependencyInjection` |
| `FlowMapper.Cli` | `FlowMapper.Abstractions`, `FlowMapper.Core` |
| `FlowMapper.Analyzers` | `FlowMapper.Abstractions` |

### 8. Instalar Pacotes NuGet

Projetos Source Generator:
- `Microsoft.CodeAnalysis.CSharp` (4.8+)
- `Microsoft.CodeAnalysis.Analyzers` (3.3+)

Projetos de teste:
- `xunit`, `xunit.runner.visualstudio`
- `FluentAssertions`
- `Microsoft.NET.Test.Sdk`
- `Verify.SourceGenerators` (snapshot tests)
- `BenchmarkDotNet` (benchmark)

### 9. Configurar Source Generator no `.csproj`
- `FlowMapper.SourceGenerator.csproj`:
  ```xml
  <EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
  <IsRoslynComponent>true</IsRoslynComponent>
  <OutputItemType>Analyzer</OutputItemType>
  ```

### 10. Verificar Build
```bash
dotnet restore FlowMapper.sln
dotnet build FlowMapper.sln
```

## Critérios de Aceitação

- [ ] `dotnet build` compila sem erros
- [ ] Todos os projetos estão na solution
- [ ] Dependências entre projetos estão corretas
- [ ] Pacotes NuGet estão restaurados
- [ ] Source Generator configurado como Analyzer

## Referências

- `Docs/Architecture/Estrutura-Soluction.md` — estrutura completa de diretórios
- `Docs/Spec/SPEC.md` seção 4 — pacotes NuGet definidos
- `Docs/Architecture/1. IIncrementalGenerator.md` — entry point do generator
