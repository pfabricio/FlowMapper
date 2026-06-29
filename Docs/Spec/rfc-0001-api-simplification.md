### 📘 Spec — RFC-0001: Public API Simplification

**Versão:** 2.0
**Status:** Aprovado (ADR-0007 revisado)
**Baseado em:** RFC-0001, ADR-0007

----------

### 1. Objetivo

Tornar `ProfileDefinition` a única API pública do FlowMapper, escondendo `[Map<,>] partial class` do usuário sem removê-lo internamente.

----------

### 2. Comportamento Atual

O Source Generator descobre candidatos de duas formas:

**Pipeline A — `[Map<,>] partial class`:**
```csharp
[Map<User, UserDto>]
public partial class UserMapper;
```

**Pipeline B — `ProfileDefinition`:**
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

Ambos geram o mesmo código. Ambos coexistem no `FlowMapperGenerator.cs`.

----------

### 3. Comportamento Alvo (pós-implementação)

```csharp
// ✅ Único jeito documentado — ProfileDefinition
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

```csharp
// ⚠️ Ainda compila, mas escondido — [Map<,>] partial class
[Map<User, UserDto>]         // ← [EditorBrowsable(Never)] + [Obsolete]
public partial class UserMapper;  // ← warning de deprecação
```

----------

### 4. O que muda no Source Generator

### 4.1 `FlowMapperGenerator.cs`

| Antes | Depois |
|-------|--------|
| Itera `[Map<,>]` candidates e `ProfileDefinition` candidates separadamente | **Igual** — ambos os pipelines são mantidos |
| Funde ambos em uma lista única | **Igual** — ambos são processados |

**Pipeline de `[Map<,>]` não é removido.** É mantido como mecanismo interno.

### 4.2 `MappingCandidateFactory.cs`

| Antes | Depois |
|-------|--------|
| `CreateFromAttribute()` — extrai de `[Map<,>]` | **Manter** (uso interno) |
| `CreateFromProfile()` — extrai de `ProfileDefinition` | **Manter** |

### 4.3 `MappingCandidate.cs`

**Sem mudanças.** `MapperName` e `ProfileName` continuam existindo.

----------

### 5. O que muda nas Abstractions

### 5.1 `MapAttribute<TSource, TDestination>`

Adicionar `[EditorBrowsable(Never)]` e `[Obsolete]`:

```csharp
[EditorBrowsable(EditorBrowsableState.Never)]
[Obsolete("Use ProfileDefinition.CreateMap<T1, T2>() instead. " +
          "See https://flowmapper.dev/docs/migration-v1")]
public class MapAttribute<TSource, TDestination> : Attribute { }
```

### 5.2 `IMapper<TSource, TDestination>`

**Não muda.** Interface runtime pública.

### 5.3 `FlowProfileAttribute`

**Não muda.** Opcional para configurar perfil.

----------

### 6. O que muda nos Analyzers

### 6.1 `MapAttributeAnalyzer.cs`

| Antes | Depois |
|-------|--------|
| Valida `[Map<,>]` + `IMapper<,>` coexistência | Suprimir warning (o `[Obsolete]` já avisa) |
| Emite FM1001 | Pode ser removido (obsoleto cobre) |

### 6.2 `AddMapAttributeCodeFixProvider.cs`

| Antes | Depois |
|-------|--------|
| Code fix que adiciona `[Map<,>]` | **Remover** (ninguém deve adicionar `[Map<,>]` novo) |

### 6.3 Novo Code Fix: `MigrateToProfileCodeFixProvider`

Converter automaticamente:
```csharp
[Map<User, UserDto>]
public partial class UserMapper;
```
para:
```csharp
public class MeuProfile : ProfileDefinition
{
    public MeuProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

----------

### 7. O que muda nos Samples

| Sample | Ação |
|--------|------|
| `samples/BasicMapping` | Migrar de `[Map<,>]` para `ProfileDefinition` |
| `samples/NestedMapping` | Migrar |
| `samples/DependencyInjection` | Migrar |
| `samples/Profiles` | Já usa `ProfileDefinition` — revisar |
| `samples/FlattenMapping` | Migrar |
| `samples/ConstructorMapping` | Migrar |
| `samples/Benchmark` | Migrar |

**Todos os samples devem usar exclusivamente `ProfileDefinition`.**

----------

### 8. O que muda nos Testes

| Teste | Ação |
|-------|------|
| `GeneratorTests` — testes com `[Map<,>]` | **Manter** (valida que pipeline interno ainda funciona) |
| `IntegrationTests` — testes com `[Map<,>]` | **Manter** (backward compat) |
| `SnapshotTests` | Atualizar snapshots se output mudar |
| `UnitTests` | Revisar |

Testes com `[Map<,>]` devem continuar passando (backward compatibility garantida).

----------

### 9. Tabela de Compatibilidade

| Cenário | Antes | Depois |
|---------|-------|--------|
| Usuário novo | `[Map<A,B>]` ou `ProfileDefinition` | `ProfileDefinition` (único documentado) |
| Usuário existente com `[Map<,>]` | Funciona | Funciona com `[Obsolete]` warning |
| Profile com políticas | `ProfileDefinition` + `[FlowProfile]` | `ProfileDefinition` + `[FlowProfile]` (igual) |

----------

### 10. Plano de Deployment

1. **Release N**: `[EditorBrowsable(Never)]` + `[Obsolete]` em `[Map<,>]`, samples migrados, docs atualizados.
2. **Release N+1**: Avaliar remoção do atributo e pipeline.
3. Code fix provider: `[Map<,>]` → `ProfileDefinition`.

---

