### 📄 ADR-0007 — Public API Simplification

**Status:** Accepted (revisado)

**Data:** 2026-06-29

**Baseado em:** RFC-0001

----------

### Contexto

O FlowMapper atualmente oferece duas maneiras de declarar um mapper:

**Opção A — `[Map<,>] partial class`:**
```csharp
[Map<User, UserDto>]
public partial class UserMapper;
```

**Opção B — `ProfileDefinition`:**
```csharp
public class ApiProfile : ProfileDefinition
{
    public ApiProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

Ambas produzem exatamente o mesmo resultado.

----------

### Problema

A coexistência de duas APIs equivalentes gera:

-   **Duplicidade de documentação** — cada recurso precisa ser explicado duas vezes.
-   **Dúvida no usuário** — qual das duas usar? Quando escolher uma ou outra?
-   **Dificuldade de extensão** — novas features (AfterMap, ConstructUsing, MapFrom lambda) só funcionam com ProfileDefinition.

A Opção A (`partial class`) não suporta configuração adicional — o usuário não pode adicionar `ForMember`, `Ignore`, `AfterMap`, etc. É uma API limitada.

----------

### Decisão

**FlowMapper orientado a Profile — `ProfileDefinition` é a única API pública.**

### O que muda visível ao usuário

1. `ProfileDefinition` passa a ser a **única** forma de declarar mapeamentos.
2. `[Map<TSource, TDestination>]` é **escondido** do usuário:
   - Marcado com `[EditorBrowsable(Never)]` — não aparece no IntelliSense
   - Marcado com `[Obsolete]` — quem já usa recebe warning para migrar
3. `IMapper<TSource, TDestination>` e `[FlowProfile]` continuam públicos (runtime).

### O que muda internamente

1. O pipeline de `[Map<,>]` no Source Generator **permanece** — é usado internamente pelo `ProfileDefinition`.
2. `CreateMap<T1, T2>()` internamente equivale a detectar `[Map<T1, T2>]`.
3. Novas features (`AfterMap`, `MapFrom`, `ConstructUsing`) **só** funcionam com `ProfileDefinition`.

----------

### API Resultante

```csharp
// ✅ Única forma de declarar mapeamentos
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>()
            .ForMember(d => d.Name, o => o.MapFrom(s => s.FullName))
            .Ignore(d => d.Password);

        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Quantity * s.UnitPrice));
    }
}
```

```csharp
// Consumo (runtime) — inalterado
var dto = mapper.Map<User, UserDto>(user);
public class Service(IMapper<User, UserDto> mapper) { ... }
```

----------

### Benefícios

-   API única e consistente (Profile-First).
-   Documentação simplificada (um caminho só).
-   Extensível por natureza — `ForMember`, `Ignore`, `MapFrom`, `AfterMap`, `ConstructUsing` já funcionam.
-   **Zero breaking change** — `[Map<,>]` continua funcionando, apenas escondido.
-   Alinhado com ADR-0006 (Explicit over Implicit).

----------

### Custos

-   **Manutenção do pipeline legado**: o pipeline de `[Map<,>]` precisa ser mantido internamente.
-   **Mitigação**: code fix provider converte `[Map<,>]` → `ProfileDefinition` automaticamente.

----------

### Plano de Migração

1. Marcar `[Map<,>]` com `[EditorBrowsable(Never)]` e `[Obsolete]`.
2. Samples e docs: 100% `ProfileDefinition`.
3. Code fix provider: `[Map<,>]` → `ProfileDefinition` automático.
4. Releases futuros: avaliar remoção total do atributo.

---

