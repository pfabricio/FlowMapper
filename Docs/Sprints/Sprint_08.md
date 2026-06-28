# Sprint 08 — Profile System

**Baseado em:** `Docs/Engines/8. PROFILE SYSTEM.md`, `Docs/Spec/SPEC.md` (seção 5.4)

## Objetivo

Implementar o Profile System para permitir regras de mapeamento diferentes por contexto (Api, Domain, Integration) com resolução em cascata.

---

## Tarefas

### 1. `ProfileDefinition` — Modelo
```csharp
public class ProfileDefinition
{
    public string Name { get; set; }
    public bool EnableFlatten { get; set; }
    public bool PreferConstructor { get; set; }
    public bool StrictMapping { get; set; }
    public bool RenameAllowed { get; set; } // v2
}
```

### 2. `ProfileRegistry`
```csharp
public static class ProfileRegistry
{
    private static readonly Dictionary<string, ProfileDefinition> _profiles = new();

    public static void Register(ProfileDefinition profile);
    public static ProfileDefinition Get(string name);
    public static bool Exists(string name);

    // Built-in profiles
    public static readonly ProfileDefinition Api = new()
    {
        Name = "Api",
        EnableFlatten = false,
        PreferConstructor = false,
        StrictMapping = false
    };

    public static readonly ProfileDefinition Domain = new()
    {
        Name = "Domain",
        EnableFlatten = true,
        PreferConstructor = true,
        StrictMapping = true
    };

    public static readonly ProfileDefinition Integration = new()
    {
        Name = "Integration",
        EnableFlatten = true,
        PreferConstructor = false,
        StrictMapping = true
    };
}
```

### 3. `ProfileResolver`
```csharp
public static class ProfileResolver
{
    public static string Resolve(
        INamedTypeSymbol mapper,
        Compilation compilation);
}
```

Ordem de prioridade:
1. Atributo `[Map<User, UserDto, Profile = "Api")]` no mapper
2. Atributo `[assembly: FlowProfile("Api")]` no assembly
3. Profile padrão (`Default`) do `FlowMapperOptions`

### 4. Atualização do `Flow` — ProfileName
```csharp
public class Flow
{
    // ... propriedades existentes
    public string ProfileName { get; set; } = "Default";
}
```

### 5. FlowBuilder Condicional por Profile
```csharp
if (profile.EnableFlatten)
    ApplyFlatten(flow, candidate);

if (profile.PreferConstructor)
    UseConstructorStrategy(flow, candidate);

if (profile.StrictMapping)
    EnableDiagnosticsStrictMode(flow);
```

### 6. Cache Multi-Profile
```csharp
// Key agora inclui Profile
key = $"{profile}|{source}|{destination}|{policy}"
```

### 7. Diagnósticos

**FM0012 — Profile not found:**
```
Profile 'X' is not registered
```

**FM0013 — Profile violation:**
```
Mapping violates rules defined in profile 'Api'
```

### 8. Built-in Profiles Comportamento

| Profile | Flatten | Constructor | Strict |
|---|---|---|---|
| `Api` | ❌ | ❌ | ❌ |
| `Domain` | ✔ | ✔ | ✔ |
| `Integration` | ✔ | ❌ | ✔ |

## Critérios de Aceitação

- [ ] `[Map<User, UserDto, "Api")]` aplica regras do perfil Api
- [ ] `[assembly: FlowProfile("Domain")]` funciona como fallback
- [ ] Profile Default é usado quando nenhum é especificado
- [ ] FlowBuilder muda comportamento baseado no profile
- [ ] FM0012 se profile não existe
- [ ] FM0013 se regra do profile é violada
- [ ] Cache considera profile na chave

## Referências

- `Docs/Engines/8. PROFILE SYSTEM.md` — ProfileDefinition, ProfileResolver, exemplos
- `Docs/Spec/SPEC.md` seção 5.4 — profiles e comportamentos
- `Docs/Spec/FlowMapper Terminology.md` — definição de Profile

## Dependências

- Sprint 02 — pipeline base
- Sprint 07 — cache multi-profile
