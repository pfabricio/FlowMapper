### 📄 ADR-0008 — Strongly Typed Callbacks

**Status:** Accepted

**Data:** 2026-06-29

**Baseado em:** RFC-0002

----------

### Contexto

Atualmente `AfterMap` e `ConstructUsing` aceitam `string methodName`:

```csharp
.AfterMap(nameof(CalcularFrete))
.ConstructUsing(nameof(CriarUserDto))
```

O Source Generator extrai o nome do método como string e o emite no código gerado.

Problemas:
-   **Sem type safety**: `nameof()` valida que o método existe, mas **não** valida a assinatura (parâmetros, retorno).
-   **Sem refactoring automático**: renomear o método não atualiza `nameof()` (apesar do IDE ajudar, não é garantido).
-   **Sem IntelliSense**: o desenvolvedor precisa saber o nome do método.
-   **String-based**: foge do espírito do FlowMapper que preza por APIs explícitas e seguras.

----------

### Proposta do RFC

```csharp
.AfterMap(CalcularFrete)
// ou
.AfterMap((s, d) => d.Frete = s.Peso * 0.5m)
```

```csharp
.ConstructUsing(CriarUserDto)
// ou
.ConstructUsing(s => new UserDto(s.Id, s.Name))
```

----------

### Decisão

**Aceitar expressões lambda fortemente tipadas como entrada, com fallback para delegates nomeados.**

### Regras

1.  `AfterMap` aceita `Expression<Action<TSource, TDestination>>`:
    -   Lambda inline: `.AfterMap((s, d) => d.Total = s.Quantity * s.UnitPrice)`
    -   Método nomeado: `.AfterMap(CalcularFrete)` (method group convertido)
2.  `ConstructUsing` aceita `Expression<Func<TSource, TDestination>>`:
    -   Lambda inline: `.ConstructUsing(s => new Dto(s.Id, s.Name))`
    -   Método nomeado: `.ConstructUsing(CriarUserDto)` (method group convertido)
3.  O **Source Generator** analisa a sintaxe da expressão no construtor do `ProfileDefinition` (já faz isso para `MapFrom`) e:
    -   Se for **lambda inline**: extrai o corpo e emite diretamente no `.g.cs`
    -   Se for **method group**: resolve o símbolo do método e emite a chamada

----------

### Exemplos de Código Gerado

### Lambda inline
```csharp
// User code
CreateMap<Order, OrderDto>()
    .ForMember(d => d.Total, o => o.MapFrom(s => s.Quantity * s.UnitPrice))
    .AfterMap((s, d) => d.Frete = s.Peso * 0.5m);

// Generated .g.cs
public OrderDto Map(Order source)
{
    var target = new OrderDto();
    target.Total = source.Quantity * source.UnitPrice;
    target.Frete = source.Peso * 0.5m;
    return target;
}
```

### Method group
```csharp
// User code
CreateMap<Order, OrderDto>()
    .AfterMap(CalcularFrete);

// Generated .g.cs
public OrderDto Map(Order source)
{
    var target = new OrderDto();
    CalcularFrete(source, target);
    return target;
}
```

----------

### Benefícios

-   ✅ **Compile-time safety**: assinatura do callback é validada pelo compilador.
-   ✅ **Refactoring automático**: renomear o método atualiza todas as referências.
-   ✅ **IntelliSense**: o IDE mostra os parâmetros disponíveis.
-   ✅ **Inline conveniente**: lambdas curtas sem criar método separado.
-   ✅ **Consistência**: mesma abordagem do `MapFrom(lambda)` já implementado.

----------

### Custos

-   **Source Generator mais complexo**: precisa analisar `ExpressionSyntax` para lambda inline e method group.
-   **Perda do método separado**: inline lambdas no profile não podem ser reutilizadas.
-   **Suporte a ambos os formatos**: dobra os casos de teste no parser do SG.

----------

### Compatibilidade

Retroceder `AfterMap(string)` → `AfterMap(Expression<...>)` é **breaking change** na API fluente.

Para suavizar:
-   Manter `AfterMap(string)` como overload por 1 release com `[Obsolete]`.
-   Code fix provider migra `.AfterMap(nameof(X))` → `.AfterMap(X)`.

---

