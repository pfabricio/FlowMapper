# Sprint 05 — Constructor & Immutable Mapping

**Baseado em:** `Docs/Engines/5. CONSTRUCTOR & IMMUTABLE MAPPING.md`, `Docs/Spec/SPEC.md` (seção 5.2)

## Objetivo

Implementar suporte a mapeamento de tipos imutáveis (records, classes sem setter público, init-only properties) via constructor binding automático.

---

## Tarefas

### 1. Detecção de Imutabilidade
```csharp
public static bool IsImmutableType(INamedTypeSymbol type)
{
    // v1:
    // - não tem setter público em propriedades necessárias
    // - é record
    // - possui construtor com parâmetros
}
```

### 2. `ConstructorResolver`
Algoritmo de resolução de construtor:
1. Obter todos os construtores públicos do tipo de destino
2. Para cada construtor, calcular score = número de parâmetros com match de nome + tipo na origem
3. Escolher construtor com maior score
4. Se nenhum construtor compatível → fallback para object initializer + warning FM0007

### 3. Regra de Match de Parâmetros (v1)
```csharp
// Match se:
// - Nome do parâmetro (case-insensitive) == nome da propriedade de origem
// - Tipo do parâmetro == tipo da propriedade de origem
```

### 4. `ConstructorBinding` — Modelo
```csharp
public class ConstructorBinding
{
    public string ParameterName { get; set; }
    public string SourceProperty { get; set; }
    public int Index { get; set; }
    public ITypeSymbol ParameterType { get; set; }
}
```

### 5. Atualização do FlowBuilder
Fase 1 — Detectar propriedades setáveis:
```csharp
if (destProp.SetMethod != null && destProp.SetMethod.DeclaredAccessibility == Accessibility.Public)
    use AssignmentStrategy
```

Fase 2 — Fallback para construtor:
```csharp
else
    map via ConstructorBinding
```

### 6. Code Generator — 3 Modos

**Modo 1 — Mutável (object initializer):**
```csharp
return new UserDto { Id = source.Id, Name = source.Name };
```

**Modo 2 — Imutável (constructor):**
```csharp
return new UserDto(source.Id, source.Name);
```

**Modo 3 — Híbrido (constructor + init):**
```csharp
return new UserDto(source.Id) { Name = source.Name };
```

### 7. Diagnósticos

**FM0007 — Constructor mismatch:**
```
No suitable constructor found for type 'UserDto'
```

**FM0008 — Missing constructor binding:**
```
Required constructor parameter 'Id' not mapped
```

### 8. Casos Suportados
| Caso | Exemplo | Estratégia |
|---|---|---|
| Record posicional | `record UserDto(int Id, string Name)` | Constructor |
| Record com propriedades | `record UserDto { int Id { get; init; } }` | Híbrido |
| Init-only | `int Id { get; init; }` | Híbrido |
| Classe sem setter | Construtor obrigatório | Constructor |
| Classe com setters | `int Id { get; set; }` | Direct |

## Critérios de Aceitação

- [ ] Records são mapeados via constructor
- [ ] Init-only properties funcionam em modo híbrido
- [ ] Classe sem setter público usa constructor binding
- [ ] FM0007 quando não há construtor compatível
- [ ] FM0008 quando parâmetro obrigatório não é mapeado
- [ ] Mix constructor + setters gera código híbrido

## Referências

- `Docs/Engines/5. CONSTRUCTOR & IMMUTABLE MAPPING.md` — algoritmo de resolução, casos, diagnósticos
- `Docs/Spec/SPEC.md` seção 5.2 — constructor mapping features
- `Docs/Spec/FlowMapper Terminology.md` — definição de Constructor Binding e Mapping Strategy

## Dependências

- Sprint 02 — pipeline base
- Sprint 03 — diagnósticos FM0007, FM0008
