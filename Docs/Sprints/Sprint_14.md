# Sprint 14 — Samples + Documentação Final

**Baseado em:** `Docs/Architecture/Estrutura-Soluction.md` (samples seção), `Docs/Engines/11. DOCUMENTATION GENERATOR.md`, `Docs/Spec/SPEC.md`

## Objetivo

Criar projetos de exemplo para cada feature do FlowMapper e implementar o Documentation Generator para produção automática de README e documentação técnica.

---

## Samples

### 1. `samples/BasicMapping`
- Mapeamento simples por convenção
- `[Map<User, UserDto>] public partial class UserMapper;`
- Console output do resultado

### 2. `samples/NestedMapping`
- `User.Address → AddressDto`
- `User.Address.City → CityDto`
- Recursão automática

### 3. `samples/FlattenMapping`
- `User.Address.City.Name → UserDto.CityName`
- Uso de flatten automático

### 4. `samples/ConstructorMapping`
- Records: `record UserDto(int Id, string Name)`
- Init-only properties
- Modo híbrido

### 5. `samples/Profiles`
- Api, Domain e Integration profiles
- Comportamento diferente por perfil
- Demonstração de `[FlowProfile]` assembly-level

### 6. `samples/DependencyInjection`
- `AddFlowMapper()` configuration
- Injeção de `IMapper<User, UserDto>` via construtor
- ASP.NET Core minimal API

### 7. `samples/Benchmark`
- Execução de benchmark local
- Comparação com AutoMapper
- Geração de relatório

---

## Documentation Generator

### 8. `MapperDocModel`
```csharp
public class MapperDocModel
{
    public string MapperName { get; set; }
    public string SourceType { get; set; }
    public string DestinationType { get; set; }
    public string Profile { get; set; }
    public List<string> MappedProperties { get; set; } = new();
    public List<string> UnmappedProperties { get; set; } = new();
    public bool UsesConstructor { get; set; }
    public bool UsesFlatten { get; set; }
}
```

### 9. `DocumentationModel`
```csharp
public class DocumentationModel
{
    public List<MapperDocModel> Mappers { get; set; } = new();
    public List<string> Profiles { get; set; } = new();
}
```

### 10. `FlowMapperDocCollector`
```csharp
public static class FlowMapperDocCollector
{
    public static DocumentationModel Collect(FlowModel flowModel);
}
```
- Extrair metadados do FlowModel
- Identificar estratégias usadas (flatten, constructor)
- Listar propriedades mapeadas e não mapeadas

### 11. Generators de Documentação

**`ReadmeGenerator`:**
```csharp
public static class ReadmeGenerator
{
    public static string Generate(DocumentationModel model);
}
```

**`MapperDocGenerator`:**
```csharp
public static class MapperDocGenerator
{
    public static string GenerateMappersDoc(DocumentationModel model);
}
```

**`ProfileDocGenerator`:**
```csharp
public static class ProfileDocGenerator
{
    public static List<string> GenerateProfiles(DocumentationModel model);
}
```

### 12. Output Gerado
```
/generated-docs/
├── README.md           # Visão geral + lista de mappers
├── mappers.md          # Documentação detalhada por mapper
└── profiles.md         # Inventário de profiles e regras
```

---

## Documentação Final do Projeto

### 13. `README.md` (raiz do projeto)
- O que é FlowMapper
- Quick start
- Features principais
- Como usar
- Roadmap

### 14. `CHANGELOG.md`
- Histórico de versões
- v1.0.0 release notes

### 15. `LICENSE`
- MIT License

---

## Critérios de Aceitação

- [ ] Todos os samples compilam e executam
- [ ] Samples demonstram cada feature claramente
- [ ] Documentation Generator produz README.md válido
- [ ] Documentação lista mappers, profiles e estratégias
- [ ] Documentação sempre sincronizada com o código (build-time)
- [ ] README.md raiz do projeto está completo

## Referências

- `Docs/Architecture/Estrutura-Soluction.md` — seção samples e docs
- `Docs/Engines/11. DOCUMENTATION GENERATOR.md` — modelo, collector, generators
- `Docs/Spec/SPEC.md` seção 5.9 — documentation generator

## Dependências

- Sprints 01-10 — todas as features implementadas
- Sprint 13 — CLI para comando `flowmapper docs`
