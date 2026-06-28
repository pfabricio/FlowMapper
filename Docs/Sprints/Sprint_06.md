# Sprint 06 — Flatten Mapping Engine

**Baseado em:** `Docs/Engines/6. FLATTEN MAPPING ENGINE.md`, `Docs/Spec/SPEC.md` (seção 5.3)

## Objetivo

Implementar o Flatten Mapping Engine para transformar propriedades aninhadas em propriedades planas (ex: `User.Address.City.Name → UserDto.CityName`).

---

## Tarefas

### 1. `FlattenResolver`
```csharp
public static class FlattenResolver
{
    public static List<FlattenPath> FindAllPaths(ITypeSymbol sourceType);
    public static FlattenPath? ResolvePath(ITypeSymbol sourceType, string targetPropertyName);
}
```

Algoritmo:
1. Explorar árvore de propriedades via DFS
2. Para cada nó folha (propriedade simples), registrar caminho completo
3. Para cada propriedade de destino sem match direto, buscar caminho flatten

### 2. `FlattenPath` — Modelo
```csharp
public class FlattenPath
{
    public string FullPath { get; set; }       // "Address.City.Name"
    public List<string> Segments { get; set; } // ["Address", "City", "Name"]
    public string TargetProperty { get; set; } // "CityName"
    public ITypeSymbol LeafType { get; set; }
}
```

### 3. Atualização do `FlowBuilder`
Nova estratégia de resolução (após Direct e Constructor):

```
Para cada propriedade de destino sem match:
  1. FlattenResolver.ResolvePath(sourceType, destPropertyName)
  2. Se caminho único → FlattenStrategy
  3. Se múltiplos caminhos → FM0009 (ambiguous)
  4. Se nenhum caminho → FM0010 (not found)
```

### 4. Regras de Match Flatten (v1)
- Match se nome da propriedade folha == nome da propriedade destino
- Match se tipo da folha == tipo do destino
- Apenas 1 caminho válido (ambiguidade = erro)
- Sem heurística de similaridade (ADR-0006)

### 5. Diagnósticos

**FM0009 — Ambiguous Flatten Path:**
```
Multiple paths found for property 'CityName'
```

**FM0010 — Flatten Path Not Found:**
```
No valid path found for 'CityName'
```

**FM0011 — Invalid Flatten Depth:**
```
Cycle or invalid depth detected in flatten graph
```

### 6. Code Generator Atualizado
```csharp
return new UserDto
{
    Name = source.Address.City.Name
};
```

### 7. Pipeline Atualizado
```
Syntax Discovery → Semantic Model → Flow Builder
    → Constructor Engine → Flatten Engine ← NEW
    → Policy Resolver → Validator → Code Generator
```

### 8. Casos Suportados
| Situação | Comportamento |
|---|---|
| Caminho único válido | Gera `source.A.B.C` |
| Múltiplos caminhos | Erro FM0009 |
| Nenhum caminho | Erro FM0010 |
| Profundidade > limite | Erro FM0011 |
| Tipo incompatível | Ignora (sem match) |

## Critérios de Aceitação

- [ ] `User.Address.City.Name → UserDto.Name` funciona
- [ ] DFS explora toda a árvore de propriedades
- [ ] Ambiguidade detectada com FM0009
- [ ] Caminho não encontrado reporta FM0010
- [ ] Sem heurísticas ou inferência (ADR-0006)
- [ ] Determinístico (mesmo input → mesmo output)

## Referências

- `Docs/Engines/6. FLATTEN MAPPING ENGINE.md` — algoritmo DFS, regras, diagnósticos
- `Docs/Spec/SPEC.md` seção 5.3 — flatten mapping features
- `Docs/Adr/ADR-0006` — Explicit over Implicit (ambiguidade = diagnóstico)
- `Docs/Adr/ADR-0005` — Deterministic Mapping

## Dependências

- Sprint 02 — pipeline base
- Sprint 03 — diagnósticos FM0009, FM0010, FM0011
