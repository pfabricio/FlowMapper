### 📁 FlowMapper

```
FlowMapper/
│
├── docs/
│   ├── architecture/
│   ├── benchmarks/
│   ├── design/
│   ├── diagnostics/
│   ├── examples/
│   ├── generator/
│   ├── roadmap/
│   └── spec/
│
├── samples/
│   ├── BasicMapping/
│   ├── NestedMapping/
│   ├── FlattenMapping/
│   ├── ConstructorMapping/
│   ├── Profiles/
│   ├── DependencyInjection/
│   └── Benchmark/
│
├── tests/
│   ├── FlowMapper.UnitTests/
│   ├── FlowMapper.Generator.Tests/
│   ├── FlowMapper.IntegrationTests/
│   ├── FlowMapper.Benchmark.Tests/
│   └── FlowMapper.SnapshotTests/
│
├── src/
│   │
│   ├── FlowMapper/
│   │
│   ├── FlowMapper.Abstractions/
│   │
│   ├── FlowMapper.DependencyInjection/
│   │
│   ├── FlowMapper.SourceGenerator/
│   │
│   ├── FlowMapper.Analyzers/
│   │
│   └── FlowMapper.Cli/
│
├── build/
│
├── tools/
│
├── .github/
│
├── FlowMapper.sln
│
├── README.md
│
├── LICENSE
│
├── CHANGELOG.md
│
└── Directory.Build.props
```

----------

### 📦 src/

É aqui que mora o framework.

```
src
│
├── FlowMapper
│
├── FlowMapper.Abstractions
│
├── FlowMapper.DependencyInjection
│
├── FlowMapper.SourceGenerator
│
├── FlowMapper.Analyzers
│
└── FlowMapper.Cli
```

----------

### 📦 FlowMapper

Pacote principal.

```
FlowMapper
│
├── Configuration/
│
├── Registration/
│
├── Internal/
│
└── FlowMapper.csproj
```

Responsável por:

-   API pública
-   Facade
-   Builder
-   Configuração

----------

### 📦 FlowMapper.Abstractions

```
FlowMapper.Abstractions
│
├── Attributes/
│
├── Interfaces/
│
├── Options/
│
├── Profiles/
│
├── Diagnostics/
│
└── FlowMapper.Abstractions.csproj
```

Exemplo:

```
Attributes
    MapAttribute

Interfaces
    IMapper

Options
    FlowMapperOptions

Profiles
    FlowProfileAttribute
```

Nada aqui depende do Roslyn.

----------

### 📦 FlowMapper.SourceGenerator

Esse é o coração.

```
FlowMapper.SourceGenerator
│
├── Discovery/
│
├── Semantic/
│
├── Model/
│
├── Builder/
│
├── Engines/
│
├── Policies/
│
├── Validation/
│
├── Generation/
│
├── Performance/
│
├── Diagnostics/
│
├── Utils/
│
└── FlowMapperIncrementalGenerator.cs
```

----------

### Discovery

```
Discovery
│
├── SyntaxReceiver
├── MapperCandidateFinder
└── CandidateCollector
```

----------

### Semantic

```
Semantic
│
├── SymbolResolver
├── TypeResolver
└── AttributeReader
```

----------

### Model

```
Model
│
├── FlowModel
├── PropertyFlow
├── ConstructorBinding
├── FlattenPath
├── NestedFlow
└── FlowSignature
```

Esse namespace é extremamente importante.

Ele representa a linguagem interna do FlowMapper.

----------

### Builder

```
Builder
│
├── FlowBuilder
├── NestedFlowBuilder
├── FlattenFlowBuilder
└── ConstructorFlowBuilder
```

----------

### Engines

```
Engines
│
├── Nested/
│
├── Flatten/
│
├── Constructor/
│
├── Profiles/
│
└── Cache/
```

Cada Engine possui:

```
Engine
Resolver
Validator
```

Exemplo:

```
Flatten
FlattenEngine
FlattenResolver
FlattenValidator
```

----------

### Policies

```
Policies
│
├── ConventionPolicy
├── NamingPolicy
├── ConstructorPolicy
└── FlattenPolicy
```

----------

### Validation

```
Validation
│
├── FlowValidator
├── CycleValidator
├── ConstructorValidator
└── PropertyValidator
```

----------

### Generation

```
Generation
│
├── CodeWriter
├── MapperGenerator
├── MethodGenerator
├── ConstructorGenerator
├── PropertyGenerator
└── FileEmitter
```

Observe que eu separaria **escrever código** de **decidir como mapear**. Essa separação facilita testes e evolução.

----------

### Performance

```
Performance
│
├── FlowCache
├── SignatureGenerator
└── IncrementalCache
```

----------

### Diagnostics

```
Diagnostics
│
├── DiagnosticCatalog
├── DiagnosticFactory
└── DiagnosticDescriptors
```

Todos os FM0001, FM0002... ficam aqui.

----------

### 📦 FlowMapper.Analyzers

Eu faria um projeto separado.

Por quê?

Porque Source Generator e Analyzer têm responsabilidades diferentes.

O Generator cria código.

O Analyzer orienta o desenvolvedor.

```
Analyzers
│
├── Rules/
│
├── CodeFixes/
│
├── Diagnostics/
│
└── Registration/
```

Assim você pode oferecer correções rápidas no Visual Studio, como:

> "Generate missing mapping"

> "Add Ignore attribute"

> "Property not mapped"

----------

### 📦 FlowMapper.DependencyInjection

```
DependencyInjection
│
├── Extensions/
└── Registration/
```

Apenas isso.

----------

### 📦 FlowMapper.Cli

Aqui começa algo que eu acho que pode ser um grande diferencial.

```
FlowMapper.Cli
│
├── Commands/
│
├── Export/
│
├── Graph/
│
├── Diagnostics/
│
└── Program.cs
```

Comandos imaginados:

```bash
flowmapper graph
```

```bash
flowmapper benchmark
```

```bash
flowmapper diagnostics
```

```bash
flowmapper docs
```

----------

### 🧪 Tests

Eu adicionaria um projeto que pouca gente faz.

```
FlowMapper.SnapshotTests
```

Para validar o código gerado.

Exemplo:

```
Expected
UserMapper.g.cs
Generated
UserMapper.g.cs
```

Se mudar uma linha sem querer, o teste acusa imediatamente. Para um Source Generator, isso é extremamente valioso.

---
