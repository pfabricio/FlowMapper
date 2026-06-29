### 📘 Spec — RFC-0002: Strongly Typed Callbacks

**Versão:** 1.0
**Status:** Aprovado (ADR-0008)
**Baseado em:** RFC-0002, ADR-0008

----------

### 1. Objetivo

Substituir `AfterMap(string)` e `ConstructUsing(string)` por overloads com `Expression<...>` para obter type safety, IntelliSense e refactoring automático.

----------

### 2. API Alvo

### 2.1 `MappingExpression<TSource, TDestination>`

```csharp
// NOVO — Expression-based (strongly typed)
public MappingExpression<TSource, TDestination> AfterMap(
    Expression<Action<TSource, TDestination>> expression);

public MappingExpression<TSource, TDestination> ConstructUsing(
    Expression<Func<TSource, TDestination>> expression);
```

```csharp
// OBSOLETO — String-based (remover no próximo release)
[Obsolete("Use the expression overload instead")]
public MappingExpression<TSource, TDestination> AfterMap(string methodName);

[Obsolete("Use the expression overload instead")]
public MappingExpression<TSource, TDestination> ConstructUsing(string methodName);
```

### 2.2 Internals

```csharp
// MappingExpression armazena o corpo da expressão como string
// (o Source Generator lê da sintaxe, não da expressão compilada)
internal string? AfterMapMethod { get; set; }
internal string? ConstructUsingMethod { get; set; }
```

A implementação dos novos métodos `Expression<...>` extrai o body via `.ToString()` como fallup runtime, mas o **verdadeiro parsing é feito pelo Source Generator** lendo a sintaxe Roslyn.

----------

### 3. Source Generator — Parsing

### 3.1 Casos a tratar

| Forma | Sintaxe | Extração |
|-------|---------|----------|
| Lambda inline | `.AfterMap((s, d) => d.Frete = s.Peso * 0.5m)` | Extrair corpo `d.Frete = s.Peso * 0.5m` |
| Method group | `.AfterMap(CalcularFrete)` | Resolver símbolo `CalcularFrete` → nome do método |
| Lambda + chamada | `.AfterMap(s => Metodo(s))` | Extrair expressão `Metodo(s)` |

### 3.2 Algoritmo

```csharp
// Em MappingCandidateFactory.CreateFromProfile()

case "AfterMap":
    if (call.Args.Count >= 1)
    {
        var arg = call.Args[0];
        afterMapMethod = ExtractCallbackMethod(arg, "AfterMap");
    }
    break;

case "ConstructUsing":
    if (call.Args.Count >= 1)
    {
        var arg = call.Args[0];
        constructUsingMethod = ExtractCallbackMethod(arg, "ConstructUsing");
    }
    break;
```

```csharp
private static string? ExtractCallbackMethod(ExpressionSyntax expr, string callbackName)
{
    // Caso 1: Method group — IdentifierNameSyntax
    if (expr is IdentifierNameSyntax identifier)
        return identifier.Identifier.Text;

    // Caso 2: Member access — Método estático de outra classe
    if (expr is MemberAccessExpressionSyntax memberAccess)
        return memberAccess.ToString();

    // Caso 3: Lambda inline — extrair corpo
    if (expr is SimpleLambdaExpressionSyntax lambda)
        return lambda.Body.ToString();

    if (expr is ParenthesizedLambdaExpressionSyntax parenLambda)
        return parenLambda.Body.ToString();

    // Caso 4: Desconhecido — reportar diagnóstico
    return null;
}
```

### 3.3 Geração de código

**FlowCodeGenerator** mantém a lógica atual:

```csharp
// AfterMap — emite chamada de método ou corpo inline
if (flow.AfterMapMethod != null)
{
    if (IsMethodCall(flow.AfterMapMethod))
        sb.AppendLine($"        {flow.AfterMapMethod}(source, target);");
    else
        sb.AppendLine($"        {flow.AfterMapMethod}");
}

// ConstructUsing — emite chamada de método ou corpo inline
if (flow.ConstructUsingMethod != null)
{
    if (IsMethodCall(flow.ConstructUsingMethod))
        sb.AppendLine($"        var target = {flow.ConstructUsingMethod}(source);");
    else
        sb.AppendLine($"        var target = {flow.ConstructUsingMethod}");
}
```

Onde `IsMethodCall` verifica se o texto parece uma chamada de método (contém `(`) ou é um corpo de lambda.

----------

### 4. O que muda em cada camada

### 4.1 `FlowMapper.Core/MappingExpression.cs`

| Campo | Antes | Depois |
|-------|-------|--------|
| `AfterMapMethod` | `internal string?` | **Manter** (continua string internamente) |
| `ConstructUsingMethod` | `internal string?` | **Manter** |
| `AfterMap(string)` | Público | `[Obsolete]` + novo overload `Expression<...>` |
| `ConstructUsing(string)` | Público | `[Obsolete]` + novo overload `Expression<...>` |

### 4.2 `FlowMapper.SourceGenerator/MappingCandidateFactory.cs`

| Método | Antes | Depois |
|--------|-------|--------|
| `ExtractCallbackMethod()` | Não existe | **Criar** — trata IdentifierName, MemberAccess, lambdas |
| `CollectFluentCalls()` | Extrai `.ToString().Trim('"')` | Usa `ExtractCallbackMethod()` |

### 4.3 `FlowMapper.SourceGenerator/Pipeline/Generator/FlowCodeGenerator.cs`

| Seção | Antes | Depois |
|-------|-------|--------|
| AfterMap emission | `{methodName}(source, target)` | Suporta lambda inline + method group |
| ConstructUsing emission | `{methodName}(source)` | Suporta lambda inline + method group |

### 4.4 `FlowMapper.Core/Flow.cs`

**Sem mudanças** — `AfterMapMethod` e `ConstructUsingMethod` continuam `string?`.

----------

### 5. Exemplos de Uso

### Lambda inline — AfterMap
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<Order, OrderDto>()
            .ForMember(d => d.Total, o => o.MapFrom(s => s.Quantity * s.UnitPrice))
            .AfterMap((s, d) => d.Frete = s.Peso * 0.5m);
    }
}
```

### Method group — AfterMap
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<Order, OrderDto>()
            .AfterMap(CalcularFrete);
    }

    private static void CalcularFrete(Order source, OrderDto dest)
        => dest.Frete = source.Peso * 0.5m;
}
```

### Lambda inline — ConstructUsing
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<Order, OrderDto>()
            .ConstructUsing(s => new OrderDto
            {
                Id = s.Id,
                Total = s.Quantity * s.UnitPrice
            });
    }
}
```

### Method group — ConstructUsing
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<Order, OrderDto>()
            .ConstructUsing(CriarOrderDto);
    }

    private static OrderDto CriarOrderDto(Order source)
        => new() { Id = source.Id };
}
```

----------

### 6. Testes

| Teste | Cenário |
|-------|---------|
| AfterMap com lambda inline | Extrair corpo e emitir no .g.cs |
| AfterMap com method group | Extrair nome e emitir chamada |
| AfterMap com método de outra classe | MemberAccess como `Helper.CalcularFrete` |
| ConstructUsing com lambda inline | Extrair corpo e emitir |
| ConstructUsing com method group | Extrair nome e emitir |
| Compatibility: string overload obsoleto | Warning de `[Obsolete]` |
| Lambda com múltiplas statements | `(s, d) => { d.X = s.X; d.Y = s.Y; }` |

----------

### 7. Compatibilidade

| Uso atual | Status |
|-----------|--------|
| `.AfterMap(nameof(Metodo))` | Compila com warning `[Obsolete]` |
| `.AfterMap("Metodo")` | Compila com warning `[Obsolete]` |
| `.AfterMap(Metodo)` (method group) | **Novo** — strongly typed |
| `.AfterMap((s,d) => ...)` (lambda) | **Novo** — strongly typed |

Migração automatizada via code fix provider no release seguinte.

---

