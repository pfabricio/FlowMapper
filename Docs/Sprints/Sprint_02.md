# Sprint 02 — Source Generator MVP

**Baseado em:** `Docs/Spec/Pred-v1.md`, `Docs/Architecture/1. IIncrementalGenerator.md`, `Docs/Architecture/Estrutura-Soluction.md`, `Docs/Spec/SPEC.md`

## Objetivo

Implementar o pipeline completo do Source Generator com mapeamento por convenção (mesmo nome + mesmo tipo), gerando `.g.cs` com object initializer. Este sprint entrega o MVP funcional do FlowMapper.

---

## Tarefas

### 1. `FlowMapperGenerator` — Entry Point
```csharp
[Generator]
public class FlowMapperGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Syntax provider → Semantic transform → Pipeline → Emit
    }
}
```
- Implementar `IIncrementalGenerator`
- Pipeline de 4 estágios

### 2. Syntax Provider — `IsMapperClass`
- Filtrar `ClassDeclarationSyntax` que:
  - Tenha `AttributeLists.Count > 0`
  - Seja `partial` (`Modifiers` contém "partial")
- **Sem resolver semantic model ainda** (filtro leve)

### 3. Semantic Transform — `GetSemanticModel`
- Resolver `INamedTypeSymbol` da classe
- Encontrar `MapAttribute` nos atributos
- Extrair `SourceType` e `DestinationType` dos argumentos genéricos
- Retornar `MappingCandidate`

### 4. `MappingCandidate`
```csharp
public class MappingCandidate
{
    public INamedTypeSymbol SourceType { get; init; }
    public INamedTypeSymbol DestinationType { get; init; }
    public INamedTypeSymbol MapperType { get; init; }
    public AttributeData Attribute { get; init; }
}
```

### 5. `MappingCandidateFactory`
- Método estático `Create(INamedTypeSymbol, AttributeData) → MappingCandidate`
- Extrair tipos dos argumentos genéricos do `MapAttribute`

### 6. `FlowPipeline.Execute`
```csharp
public static FlowModel Execute(IReadOnlyList<MappingCandidate> candidates)
```
- Orquestrar pipeline: para cada candidate → FlowBuilder → collect → FlowModel

### 7. `FlowBuilder.Build` — Convention Engine v1
```csharp
public static Flow Build(MappingCandidate candidate)
```
Para cada propriedade de origem:
- Se destino tiver propriedade de **mesmo nome**
- **E** mesmo tipo (`SymbolEqualityComparer`)
- Adicionar `PropertyFlow` com `Strategy = MappingStrategy.Direct`

### 8. `FlowModel`
```csharp
public class FlowModel
{
    public List<Flow> Flows { get; }
    public string MapperName { get; }
}
```

### 9. `FlowCodeGenerator.Generate`
```csharp
public static string Generate(FlowModel model)
```
Gerar código C#:
```csharp
public partial class UserMapper : IMapper<User, UserDto>
{
    public UserDto Map(User source)
    {
        return new UserDto
        {
            Id = source.Id,
            Name = source.Name
        };
    }
}
```
- Implementar `IMapper<TSource, TDest>` na classe gerada
- Object initializer para propriedades mapeadas

### 10. `EmitSource`
- `context.AddSource($"{MapperName}.g.cs", code)`
- Registrar saída no Roslyn

### 11. Tratamento de Casos Especiais

| Caso | Comportamento |
|---|---|
| Propriedade sem match | Ignorar (warning em Sprint 03) |
| Tipo incompatível | Ignorar |
| Classe não partial | Ignorar no syntax provider |
| Múltiplos mappers | Processar cada um independentemente |

### 12. Teste Manual
- Criar console app de teste com:
  ```csharp
  [Map<User, UserDto>]
  public partial class UserMapper;
  ```
- Buildar e verificar `.g.cs` gerado

## Critérios de Aceitação

- [ ] Atributo `[Map<User, UserDto>]` gera `UserMapper.g.cs`
- [ ] Código gerado compila sem erros
- [ ] `IMapper<User, UserDto>` implementado corretamente
- [ ] Mapeamento por convenção (nome + tipo) funciona
- [ ] Pipeline completo: Syntax → Semantic → Build → Generate → Emit
- [ ] Múltiplos mappers no mesmo projeto funcionam

## Referências

- `Docs/Architecture/1. IIncrementalGenerator.md` — código completo do generator, pipeline, builder
- `Docs/Spec/Pred-v1.md` — regras de convenção v1 (nome exato, tipo exato)
- `Docs/Spec/SPEC.md` seção 5 — core features
- `Docs/Architecture/Estrutura-Soluction.md` — estrutura do SourceGenerator

## Dependências

- Sprint 01 — core domain models
- NuGet: `Microsoft.CodeAnalysis.CSharp`, `Microsoft.CodeAnalysis.Analyzers`
