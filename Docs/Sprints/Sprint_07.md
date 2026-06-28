# Sprint 07 — Performance & Cache Engine

**Baseado em:** `Docs/Engines/7. PERFORMANCE ENGINE.md`, `Docs/Spec/SPEC.md` (seção 5.6)

## Objetivo

Implementar o sistema de cache de Flow para evitar recálculo de mapeamentos já processados, garantindo performance incremental e reuso entre builds.

---

## Tarefas

### 1. `FlowCache`
```csharp
public class FlowCache
{
    private readonly ConcurrentDictionary<string, Flow> _cache = new();

    public bool TryGet(string key, out Flow flow);
    public void Set(string key, Flow flow);
    public void Clear();
}
```
- Thread-safe via `ConcurrentDictionary`
- Cache por sessão de compilação (sem persistência)

### 2. `FlowKeyGenerator`
```csharp
public static class FlowKeyGenerator
{
    public static string Create(
        INamedTypeSymbol source,
        INamedTypeSymbol dest,
        string profile = "Default",
        MappingPolicy? policy = null);

    public static string CreateFromSignature(FlowSignature signature);
}
```
Chave do cache:
```
{Profile}|{SourceType}|{DestinationType}|{PolicyHash}
```

### 3. `FlowSignature` — Atualizado
```csharp
public class FlowSignature
{
    public string SourceTypeId { get; set; }   // FullyQualifiedName
    public string DestinationTypeId { get; set; }
    public string ProfileName { get; set; }
    public string PolicyHash { get; set; }     // Hash estável das regras
    public string PropertyHash { get; set; }   // Hash dos nomes+tipos das propriedades

    public string ToCacheKey();
}
```

### 4. `SignatureGenerator`
```csharp
public static class SignatureGenerator
{
    public static FlowSignature Generate(Flow flow);
    public static FlowSignature GenerateFromCandidate(MappingCandidate candidate);
}
```

### 5. Integração no FlowBuilder
```csharp
public static Flow Build(MappingCandidate candidate, FlowCache cache)
{
    var key = FlowKeyGenerator.Create(candidate.SourceType, candidate.DestinationType);

    if (cache.TryGet(key, out var cachedFlow))
        return cachedFlow;

    var flow = BuildFlowInternal(candidate);
    cache.Set(key, flow);
    return flow;
}
```

### 6. Cache nos Engines
Todos os engines devem usar cache:

| Engine | Cache Key |
|---|---|
| FlowBuilder | Source + Destination + Profile + Policy |
| Nested Engine | ParentProperty + ChildFlow signature |
| Constructor Engine | Type + Constructor parameters |
| Flatten Engine | SourceType + PropertyName |

### 7. Lazy Semantic Resolution
```csharp
// Syntax → Candidate (leve) → SemanticModel só quando necessário
// Se cache hit, não resolve semantic model
```

### 8. Pipeline Atualizado
```
MappingCandidate
    ↓
Signature Generator
    ↓
FlowCache Lookup
    ↓
[HIT] → reuse Flow (sem SemanticModel)
    ↓
[MISS] → build Flow → Store in Cache
```

## Critérios de Aceitação

- [ ] Cache evita recálculo do mesmo Flow
- [ ] Chave de cache inclui Profile + Policy
- [ ] Signature hash é estável (determinístico)
- [ ] Thread-safe durante compilação
- [ ] Cache integrado nos 3 engines (nested, constructor, flatten)
- [ ] Cache não persiste entre soluções (sessão apenas)

## Referências

- `Docs/Engines/7. PERFORMANCE ENGINE.md` — FlowCache, FlowKeyGenerator, Signature, otimizações
- `Docs/Spec/SPEC.md` seção 5.6 — cache engine
- `Docs/Adr/ADR-0001` — Compile-Time First (custo do build é aceitável)
- `Docs/Adr/ADR-0005` — Deterministic Mapping (hash estável)

## Dependências

- Sprint 04 — Nested Engine (cache nested flows)
- Sprint 05 — Constructor Engine (cache constructor bindings)
- Sprint 06 — Flatten Engine (cache flatten paths)
