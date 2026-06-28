# Sprint 04 — Nested Mapping Engine

**Baseado em:** `Docs/Engines/4. NESTED MAPPING ENGINE.md`, `Docs/Spec/SPEC.md` (seção 5.1)

## Objetivo

Implementar o Nested Mapping Engine para suportar mapeamento de objetos aninhados (ex: `User.Address → AddressDto`) com recursão controlada e detecção de ciclos.

---

## Tarefas

### 1. Detecção de Tipos Complexos
```csharp
public static bool IsComplexType(ITypeSymbol type)
{
    // Complex = não primitivo + não string + não collection primária
    if (type.SpecialType != SpecialType.None) return false;
    if (type.TypeKind == TypeKind.Enum) return false;
    if (type is IArrayTypeSymbol) return false;
    return true;
}
```

### 2. `NestedFlowBuilder`
- Responsável por detectar propriedades de tipo complexo
- Para cada propriedade complexa:
  1. Verificar se existe Flow registrado para aquele par (source type, dest type)
  2. Se existir → reutilizar
  3. Se não → criar novo Flow recursivamente

### 3. Atualização do `FlowBuilder`
No `FlowBuilder.Build`, após mapear propriedades diretas:
```csharp
if (IsComplexType(sp.Type) && IsComplexType(dp.Type))
{
    var nestedFlow = BuildNestedFlow(sp.Type, dp.Type, visited);
    flow.NestedFlows.Add(nestedFlow);
}
```

### 4. Controle de Recursão — `visited` HashSet
```csharp
private static Flow BuildNestedFlow(
    ITypeSymbol sourceType,
    ITypeSymbol destType,
    HashSet<string> visited)
{
    var key = $"{sourceType.Name}→{destType.Name}";
    if (visited.Contains(key))
    {
        // ciclo detectado → FM0006
        return null;
    }
    visited.Add(key);
    // ... build nested flow
}
```

### 5. Diagnóstico — FM0006
```csharp
public static readonly DiagnosticDescriptor CyclicReference = new(
    id: "FM0006",
    title: "Cyclic reference detected",
    messageFormat: "Cycle detected in mapping path: {0}",
    category: "FlowMapper",
    DiagnosticSeverity.Error,
    isEnabledByDefault: true);
```

### 6. Code Generator Atualizado
Gerar métodos auxiliares para cada nested flow:
```csharp
public partial class UserMapper
{
    public UserDto Map(User source)
    {
        return new UserDto
        {
            Name = source.Name,
            Address = MapAddress(source.Address)
        };
    }

    private AddressDto MapAddress(Address source)
    {
        return new AddressDto
        {
            City = source.City
        };
    }
}
```

### 7. Casos Suportados
| Situação | Comportamento |
|---|---|
| 1 nível de nesting | Gera método auxiliar |
| Multinível (A→B→C) | Recursão controlada |
| Reuso de flow existente | Reaproveita método gerado |
| Ciclo (A→B→A) | Erro FM0006 |
| Tipo primitivo | Ignorado (mapeamento direto) |

## Critérios de Aceitação

- [ ] `User.Address` → `AddressDto` mapeado automaticamente
- [ ] Métodos auxiliares gerados para cada nested flow
- [ ] Reuso de flows existentes (se `AddressMapper` já existe, usa)
- [ ] Ciclo A→B→A detectado com FM0006
- [ ] 1 nível + recursão controlada funcionando

## Referências

- `Docs/Engines/4. NESTED MAPPING ENGINE.md` — algoritmo, regras, exemplos, modelo
- `Docs/Spec/SPEC.md` seção 5.1 — nested mapping features
- `Docs/Adr/ADR-0006` — Explicit over Implicit (ciclo deve gerar diagnóstico, não decisão)

## Dependências

- Sprint 02 — FlowBuilder e CodeGenerator base
- Sprint 03 — sistema de diagnósticos (FM0006)
