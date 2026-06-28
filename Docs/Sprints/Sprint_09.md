# Sprint 09 — Fluent Configuration + Dependency Injection

**Baseado em:** `Docs/Engines/9. FLUENT CONFIGURATION API.md`, `Docs/Architecture/2. RUNTIME API + DEPENDENCY INJECTION.md` (pendente), `Docs/Spec/SPEC.md` (seção 5.5)

## Objetivo

Criar a API fluente de configuração global do FlowMapper e o extension method de DI (DependencyInjection) para integração com ASP.NET Core.

---

## Tarefas

### 1. `FlowMapperConfigurationBuilder`
```csharp
public class FlowMapperConfigurationBuilder
{
    private readonly FlowMapperOptions _options = new();

    public FlowMapperConfigurationBuilder UseDefaultProfile(string profile);
    public FlowMapperConfigurationBuilder EnableFlatten(bool enabled = true);
    public FlowMapperConfigurationBuilder PreferConstructor(bool enabled = true);
    public FlowMapperConfigurationBuilder StrictMode();
    public FlowMapperConfigurationBuilder WarnMode();
    public FlowMapperConfigurationBuilder DisableCache();
    public FlowMapperConfigurationBuilder EnableCache(bool enabled = true);
    public FlowMapperOptions Build();
}
```
- Todos os métodos retornam o builder (fluent)
- `Build()` produz `FlowMapperOptions` imutável

### 2. `FlowMapperOptions` — Atualizado
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

### 3. `AddFlowMapper` Extension Method
```csharp
public static class FlowMapperServiceCollectionExtensions
{
    public static IServiceCollection AddFlowMapper(
        this IServiceCollection services,
        Action<FlowMapperConfigurationBuilder>? configure = null);
}
```
- Criar `FlowMapperConfigurationBuilder`
- Aplicar configuração do usuário
- Registrar `FlowMapperOptions` como singleton
- Registrar mappers gerados como scoped

### 4. `RegisterGeneratedMappers`
```csharp
private static void RegisterGeneratedMappers(IServiceCollection services)
{
    // Scanner de assemblies para tipos que implementam IMapper<,>
    // Registro como scoped
}
```

### 5. Camadas de Prioridade
```
1. [MapAttribute] — por mapper
2. [FlowProfileAttribute] — por profile
3. FlowMapperOptions — configuração global
4. Engine Defaults — regras fixas
```

### 6. Integração com Source Generator
O generator deve ler as opções via:
- `FlowMapperOptions` capturadas em build-time
- Atributos + constantes (simplificado v1)

### 7. Uso Final
```csharp
builder.Services.AddFlowMapper(cfg =>
{
    cfg.UseDefaultProfile("Api")
       .EnableFlatten()
       .PreferConstructor()
       .WarnMode();
});
```

### 8. Injeção do Mapper
```csharp
public class UserService
{
    private readonly IMapper<User, UserDto> _mapper;

    public UserService(IMapper<User, UserDto> mapper)
    {
        _mapper = mapper;
    }

    public UserDto GetUser(int id)
    {
        var user = _repository.Get(id);
        return _mapper.Map(user);
    }
}
```

## Critérios de Aceitação

- [ ] API fluente funcional com todos os métodos
- [ ] `AddFlowMapper()` registra options como singleton
- [ ] Mappers gerados são registrados como scoped
- [ ] Ordem de prioridade (Attribute > Profile > Options > Defaults)
- [ ] `FlowMapperOptions` pode ser injetado em outros serviços

## Referências

- `Docs/Engines/9. FLUENT CONFIGURATION API.md` — builder, options, DI extension
- `Docs/Spec/SPEC.md` seção 5.5 — fluent configuration
- `Docs/Architecture/2. RUNTIME API + DEPENDENCY INJECTION.md` — (pendente, seguir seção 9 do engine)

## Dependências

- Sprint 01 — FlowMapperOptions, StrictnessLevel, IMapper
- Sprint 08 — Profile System (para configurar profiles globalmente)
