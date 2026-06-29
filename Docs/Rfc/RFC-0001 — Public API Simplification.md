### RFC-0001 — Public API Simplification

**Objetivo**

Avaliar a remoção da API baseada em `partial class`.

### Situação Atual

```csharp
[Map<User, UserDto>]
public partial class UserMapper;
```
e
```csharp
public class ApiProfile : ProfileDefinition
{
    public ApiProfile()
    {
        CreateMap<User, UserDto>();
    }
}
```

coexistem.

### Problema

Existem duas maneiras de fazer exatamente a mesma coisa.

Isso aumenta:

-   documentação
-   testes
-   manutenção
-   dúvidas do usuário

### Proposta

Antes da versão 1.0 avaliar remover a API baseada em `partial`.

A API pública passaria a ser exclusivamente baseada em `ProfileDefinition`.

### Status

🟡 Em discussão

---