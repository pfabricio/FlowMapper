# Sprint 01 — Core Domain + Abstractions

**Baseado em:** `Docs/Spec/SPEC.md` (seções 4.1, 4.2), `Docs/Spec/FlowMapper Terminology.md`, `Docs/Architecture/1. IIncrementalGenerator.md` (modelos), `Docs/Architecture/Estrutura-Soluction.md`

## Objetivo

Criar os projetos `FlowMapper.Abstractions` e `FlowMapper.Core` que formam a base do framework. Nenhum destes projetos depende do Roslyn — são modelos puros de domínio.

---

## FlowMapper.Abstractions

### 1. `MapAttribute`
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class MapAttribute<TSource, TDestination> : Attribute { }
```
- Atributo genérico que declara um mapeamento
- Usado pelo usuário: `[Map<User, UserDto>] public partial class UserMapper;`

### 2. `IMapper<TSource, TDestination>`
```csharp
public interface IMapper<TSource, TDestination>
{
    TDestination Map(TSource source);
}
```
- Interface runtime que os mappers gerados implementam
- Permite uso com DI

### 3. `FlowMapperOptions`
```csharp
public class FlowMapperOptions
{
    public string DefaultProfile { get; set; } = "Default";
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructorMapping { get; set; } = false;
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;
    public bool EnableCache { get; set; } = true;
}
```
- Classe de configuração global
- Usada pelo Fluent Configuration API (Sprint 09)

### 4. `StrictnessLevel`
```csharp
public enum StrictnessLevel { None, Warning, Error }
```

### 5. `FlowProfileAttribute`
```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Assembly)]
public class FlowProfileAttribute : Attribute
{
    public string Name { get; }
    public FlowProfileAttribute(string name) { Name = name; }
}
```
- Atributo para definir perfil de mapeamento
- Usado em Sprint 08 (Profile System)

### 6. `MapPropertyAttribute` (opcional v1)
```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MapPropertyAttribute : Attribute
{
    public string Source { get; }
    public string Destination { get; }
}
```
- Para configuração explícita de propriedades
- Alinhado com ADR-0006 (Explicit over Implicit)

---

## FlowMapper.Core

### 7. `Flow`
```csharp
public class Flow
{
    public string SourceType { get; set; }
    public string DestinationType { get; set; }
    public string ProfileName { get; set; } = "Default";
    public List<PropertyFlow> Properties { get; set; } = new();
    public List<NestedFlow> NestedFlows { get; set; } = new();
    public List<ConstructorBinding> ConstructorBindings { get; set; } = new();
    public MappingPolicy Policy { get; set; }
}
```
- Unidade central de trabalho do FlowMapper
- Representa o mapeamento completo entre dois tipos

### 8. `PropertyFlow`
```csharp
public class PropertyFlow
{
    public string SourceProperty { get; set; }
    public string DestinationProperty { get; set; }
    public string SourcePath { get; set; }     // para flatten
    public MappingStrategy Strategy { get; set; }
    public int? ConstructorParameterIndex { get; set; }
}

public enum MappingStrategy
{
    Direct,
    Constructor,
    Nested,
    Flatten
}
```

### 9. `MappingPolicy`
```csharp
public class MappingPolicy
{
    public StrictnessLevel Strictness { get; set; } = StrictnessLevel.None;
    public bool EnableFlatten { get; set; } = true;
    public bool PreferConstructor { get; set; } = false;
}
```

### 10. `NestedFlow`
```csharp
public class NestedFlow
{
    public string ParentProperty { get; set; }
    public Flow ChildFlow { get; set; }
    public MappingStrategy Strategy { get; set; }
}
```
- Usado na Sprint 04 (Nested Mapping)

### 11. `ConstructorBinding`
```csharp
public class ConstructorBinding
{
    public string ParameterName { get; set; }
    public string SourceProperty { get; set; }
    public int Index { get; set; }
}
```
- Usado na Sprint 05 (Constructor Mapping)

### 12. `FlattenPath`
```csharp
public class FlattenPath
{
    public string FullPath { get; set; }
    public List<string> Segments { get; set; } = new();
    public string TargetProperty { get; set; }
}
```
- Usado na Sprint 06 (Flatten Mapping)

### 13. `FlowSignature`
```csharp
public class FlowSignature
{
    public string SourceTypeId { get; set; }
    public string DestinationTypeId { get; set; }
    public string ProfileName { get; set; }
    public string PolicyHash { get; set; }
}
```
- Usado na Sprint 07 (Cache Engine)

### 14. `ProfileDefinition`
```csharp
public class ProfileDefinition
{
    public string Name { get; set; }
    public bool EnableFlatten { get; set; }
    public bool PreferConstructor { get; set; }
    public bool StrictMapping { get; set; }
}
```
- Usado na Sprint 08 (Profile System)

---

## Critérios de Aceitação

- [ ] `FlowMapper.Abstractions` compila sem dependências externas
- [ ] `FlowMapper.Core` compila sem dependências do Roslyn
- [ ] Todos os modelos de domínio estão implementados
- [ ] Enums e classes seguem nomenclatura da `FlowMapper Terminology.md`
- [ ] Projetos referenciados corretamente na solution

## Referências

- `Docs/Spec/FlowMapper Terminology.md` — definição de Flow, PropertyFlow, Flow Signature, etc.
- `Docs/Spec/SPEC.md` seção 4 — pacotes Abstractions e Core
- `Docs/Architecture/1. IIncrementalGenerator.md` seções 2-3 — modelos Flow e MappingCandidate
- `Docs/Architecture/Estrutura-Soluction.md` — estrutura de diretórios dos projetos
- `Docs/Adr/ADR-0006` — Explicit over Implicit (guia para `MapPropertyAttribute`)
