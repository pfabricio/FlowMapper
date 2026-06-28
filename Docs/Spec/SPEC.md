### 📘 FLOWMAPPER v1 — SPEC OFICIAL (Compile-Time Mapping Framework)

----------

### 🧠 1. VISÃO GERAL

FlowMapper é um **framework de mapeamento de objetos em compile-time**, baseado em:

-   🧩 Roslyn Source Generators
-   🌳 Graph-based mapping engine
-   ⚡ Zero reflection em runtime
-   🧠 Pipeline determinístico de geração de código
-   📦 Arquitetura modular via NuGet packages

----------

### 🎯 2. OBJETIVO

Substituir soluções runtime (ex: AutoMapper) por um sistema:

-   ⚡ mais rápido (zero runtime expression trees)
-   🧠 mais previsível (determinístico)
-   🧱 mais seguro (compile-time validation)
-   📦 mais escalável (cache + reuse de flows)

----------

### 🏗 3. ARQUITETURA GERAL

```
User Code
   ↓
FlowMapper Attributes / Profiles
   ↓
Roslyn Incremental Generator
   ↓
Flow Builder (Graph Engine)
   ↓
Nested Engine
   ↓
Constructor Engine
   ↓
Flatten Engine
   ↓
Profile Resolver
   ↓
Validator (Diagnostics)
   ↓
Code Generator
   ↓
Generated Mappers (.g.cs)
```

----------

### 📦 4. PACOTES NUGET

### FlowMapper.Abstractions

-   IMapper<T>
-   Attributes
-   FlowMapperOptions

### FlowMapper.SourceGenerator

-   Graph engine
-   Roslyn generator
-   Flatten / Nested / Constructor engines
-   Diagnostics

### FlowMapper.DependencyInjection

-   AddFlowMapper()
-   DI registration

### FlowMapper (meta package)

-   entrypoint do usuário final

----------

### 🧩 5. CORE FEATURES

----------

### 5.1 Nested Mapping 🌳

Suporte a objetos aninhados:

```
User.Address.City.Name → UserDto.CityName
```

✔ DFS graph traversal  
✔ reuse of existing flows  
✔ cycle detection (FM0006)

----------

### 5.2 Constructor Mapping 🧱

Suporte a tipos imutáveis:

```csharp
new UserDto(id, name)
```

✔ record support  
✔ constructor binding  
✔ fallback to initializer

----------

### 5.3 Flatten Mapping 🌿

Suporte a projeção de caminhos:

```
User.Address.City.Name → CityName
```

✔ explicit path resolution  
✔ deterministic DFS  
✔ ambiguity error (FM0009)

----------

### 5.4 Profiles ⚙️

Context-aware mapping behavior:

-   Api
-   Domain
-   Integration

Ex:
| Profile | Behavior |
|---       |---        |
| Api | simple mapping |
| Domain | constructor + flatten |
| Integration | strict mapping |

----------

### 5.5 Fluent Configuration 🧪

```csharp
AddFlowMapper(cfg =>
{
    cfg.UseDefaultProfile("Api")
       .EnableFlatten()
       .PreferConstructor()
       .WarnMode();
});
```

----------

### 5.6 Cache Engine ⚡

Flow reuse via signature:

```
Source + Destination + Profile + Policy
```

✔ incremental generation  
✔ no recomputation  
✔ deterministic reuse

----------

### 5.7 Diagnostics Engine 🚨

Erros padronizados:

-   FM0006 → cyclic reference
-   FM0007 → constructor mismatch
-   FM0008 → missing binding
-   FM0009 → ambiguous flatten path
-   FM0010 → path not found
-   FM0012 → invalid profile

----------

### 5.8 Benchmark Engine 📊

Comparação com AutoMapper:

-   latency
-   memory allocation
-   GC pressure
-   throughput

----------

### 5.9 Documentation Generator 📚

Auto-generated:

-   README.md
-   mapper docs
-   profile docs

✔ always in sync with code

----------

### 5.10 Visual Graph Engine 🌐

Exporta:

-   Mermaid diagrams
-   DOT graphs

Ex:

```
User → Address → City → Name
```

----------

### ⚙️ 6. PIPELINE DE EXECUÇÃO

```
Syntax Discovery
   ↓
Semantic Model Resolution
   ↓
Flow Signature Generator
   ↓
Flow Cache Lookup
   ↓
Flow Builder
   ↓
Nested Engine
   ↓
Constructor Engine
   ↓
Flatten Engine
   ↓
Profile Resolver
   ↓
Validator
   ↓
Code Generator
   ↓
Emit .g.cs
```

----------

### 🧠 7. REGRAS FUNDAMENTAIS

-   ❌ nunca usar reflection em runtime
-   ✔ tudo determinístico
-   ✔ zero magic behavior oculto
-   ✔ compile-time validation obrigatória
-   ✔ cache baseado em signature
-   ✔ graph traversal controlado

----------

### ⚡ 8. PERFORMANCE MODEL

-   Reuso de Flow: O(1)
-   Build inicial: O(n graph depth)
-   Runtime mapping: puro C# gerado
-   GC pressure: mínimo
-   Allocation: quase zero

----------

### 🚫 9. LIMITAÇÕES (v1)

-   sem multi-tenant runtime profiles
-   sem dynamic mapping runtime
-   sem external configuration store
-   sem AI-based mapping inference

----------

### 📦 10. OUTPUT FINAL

O FlowMapper gera:

```
UserMapper.g.cs
AddressMapper.g.cs
CityMapper.g.cs
```

✔ código C# puro  
✔ otimizado pelo compilador  
✔ sem runtime overhead

----------

### 🧭 11. ROADMAP

### v1 (ATUAL)

✔ nested  
✔ flatten  
✔ constructor  
✔ profiles  
✔ cache  
✔ diagnostics  
✔ docs  
✔ benchmark

----------

### v2 (FUTURO)

-   CLI tool (`dotnet flowmapper`)
-   Web visual debugger
-   mapping rules DSL
-   rename policies
-   conditional mapping

----------

### 🧨 12. POSICIONAMENTO DO FLOWMAPPER

> FlowMapper é um framework de compile-time object mapping baseado em Roslyn, projetado para substituir soluções runtime como AutoMapper com performance determinística e validação em tempo de build.

----------

### 🔥 13. RESUMO FINAL

Você construiu um sistema que tem:

-   🧠 engine de grafo de objetos
-   ⚡ source generator avançado
-   🧩 profiles de comportamento
-   📦 cache incremental
-   🌿 flatten + nested + constructor
-   📊 benchmark system
-   📚 auto-documentation
-   🌐 visual graph export
---
