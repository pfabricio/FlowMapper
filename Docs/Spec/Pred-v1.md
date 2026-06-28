# 📄 FLOWMAPPER v1 — MVP PRD (v1.0)

## 🧠 1. Visão do Produto

**FlowMapper** é uma biblioteca .NET de mapeamento de objetos baseada em **Source Generator**, que transforma tipos de forma previsível, explícita e determinística, sem reflection em runtime.

> Objetivo do MVP: permitir mapear `Entity → DTO` com convenção automática e código gerado em `.g.cs`.

----------

### 🎯 2. Objetivo do MVP v1

Entregar um sistema funcional que:

-   Gere código C# em compile-time
-   Mapeie objetos por convenção (mesmo nome e tipo)
-   Suporte classes, records e coleções
-   Não use reflection no runtime de execução do mapper
-   Seja simples de usar e entender

----------

### 🚫 3. Fora de escopo (IMPORTANTE)

Essas features NÃO entram no v1:

-   Conversores customizados
-   Flatten avançado (Address.City → City)
-   Rename mapping
-   Pipeline (Before/After)
-   Conditions
-   Ignore explícito
-   Reverse mapping
-   Projeções LINQ
-   Config fluente
-   Explain / Validate / Diagnostics avançados

> Regra: v1 é **convention-only**

----------

### 🧱 4. Forma de uso (API do usuário)

### 4.1 Declaração do Mapper (Opção A)

```
[Map<User, UserDto>]public partial class UserMapper;
```

✔ Classe partial  
✔ Sem métodos escritos pelo usuário  
✔ Totalmente gerada pelo Source Generator

----------

### ⚙️ 5. Comportamento do sistema

### 5.1 Regra de mapeamento (CONVENÇÃO v1)

Uma propriedade será mapeada automaticamente quando:

-   Nome for exatamente igual
-   Tipo for exatamente igual
-   Propriedade de origem tiver getter público
-   Propriedade de destino tiver setter público ou construtor compatível

----------

### 5.2 Exemplos válidos

### Igualdade total

```
User.Id -> UserDto.IdUser.Name -> UserDto.Name
```

----------

### Coleções

```
List<User> -> List<UserDto>
```

----------

### Records

```
record UserDto(int Id, string Name);
```

✔ suportado via constructor binding

----------

### 🧠 6. Modelo mental interno (DOMÍNIO v1)

### 6.1 Flow (conceito base)

```
Flow
├── SourceType
├── DestinationType
├── Steps[]
└── Policy
```

----------

### 6.2 FlowStep

```
FlowStep
├── SourceMember
├── DestinationMember
└── Strategy = Convention
```

----------

### 6.3 Policy

```
FlowPolicy
├── Strictness = None (default v1)
```

### Strictness v1:

```
None → ignora inconsistências
```

----------

### ⚙️ 7. Source Generator Pipeline

```
[Map<User, UserDto>]
        ↓
MappingDiscovery (Roslyn)
        ↓
FlowBuilder
        ↓
ConventionMatcher
        ↓
FlowValidator (v1 simples)
        ↓
CodeGenerator
        ↓
UserMapper.g.cs
```

----------

### 🧾 8. Código gerado (EXEMPLO FINAL)

```csharp
public partial class UserMapper
{
    public UserDto Map(User source)
    {
       return new UserDto
       {
           Id = source.Id,
           Name = source.Name,
           Email = source.Email
       };
    }
}
```

----------

### 🧩 9. Arquitetura da solução

```
FlowMapper.slnsrc/
  ├── FlowMapper.Abstractions
  │     → Attributes, Interfaces
  │
  ├── FlowMapper.Core 
  │     → Domain (Flow, Steps, Policy)
  │
  ├── FlowMapper.SourceGenerator
  │     → Roslyn Generator
  │
  ├── FlowMapper.Analyzers (v1 vazio ou mínimo)
  │
  ├── FlowMapper.DependencyInjection (opcional v1)
```

----------

### ⚡ 10. API Runtime (mínima)

### Interface base

```
public interface IMapper<TSource, TDestination>{    TDestination Map(TSource source);}
```

----------

### Uso

```
UserDto dto = mapper.Map<UserDto>(user);
```

----------

### 🚨 11. Regras importantes do MVP

-   ❌ Nenhuma decisão implícita fora de convenção
-   ❌ Nenhuma magia de runtime
-   ✔ Tudo previsível
-   ✔ Tudo gerado em `.g.cs`
-   ✔ Código final legível como código humano

----------

### 📦 12. Entregáveis do v1

### Obrigatórios:

-   Source Generator funcional
-   Atributo `[Map<T1, T2>]`
-   Mapeamento por convenção
-   Suporte a classes
-   Suporte a records
-   Suporte a collections
-   Geração de `.g.cs`

----------

### Opcionais (se sobrar tempo):

-   DI registration (`AddFlowMapper()`)

----------

### 🧭 13. Definição de sucesso do MVP

O MVP é considerado pronto quando:

> Um desenvolvedor consegue criar um DTO a partir de uma entidade usando apenas o atributo `[Map<,>]` e o código gerado funciona sem nenhuma configuração adicional.