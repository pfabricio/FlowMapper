# Sprint 11 — Test Suite

**Baseado em:** `Docs/Architecture/Estrutura-Soluction.md` (seção Tests), `Docs/Engines/3. DIAGNOSTICS ENGINE.md` (testes de diagnóstico), `Docs/Engines/10. BENCHMARK ENGINE.md`

## Objetivo

Criar a suíte completa de testes do FlowMapper, cobrindo desde testes de unidade no domínio até testes de snapshot do código gerado.

---

## Tarefas

### 1. `FlowMapper.UnitTests`
Testes para o domínio (`FlowMapper.Core` e `FlowMapper.Abstractions`):

```csharp
// FlowTests
[Fact] public void Flow_Should_Have_Source_And_Destination_Types()
[Fact] public void Flow_Should_Add_PropertyFlow()
[Fact] public void Flow_With_NestedFlow_Should_Be_Valid()
[Fact] public void FlowSignature_Should_Be_Deterministic()

// PropertyFlowTests
[Fact] public void PropertyFlow_With_Direct_Strategy()
[Fact] public void PropertyFlow_With_Flatten_Strategy()
[Fact] public void PropertyFlow_With_Constructor_Strategy()

// MappingPolicyTests
[Fact] public void Default_Policy_Should_Be_None()
[Fact] public void Strict_Policy_Should_Set_Strictness_Error()

// ProfileDefinitionTests
[Fact] public void Api_Profile_Should_Disable_Flatten()
[Fact] public void Domain_Profile_Should_Prefer_Constructor()
```

### 2. `FlowMapper.Generator.Tests`
Testes para o Source Generator usando `Microsoft.CodeAnalysis.Testing`:

```csharp
// Geração básica
[Fact] public async Task MapAttribute_Generates_MapperClass()
[Fact] public async Task Convention_Mapping_Matches_ByName()
[Fact] public async Task Type_Mismatch_Generates_Error()
[Fact] public async Task Multiple_Mappers_All_Generated()
[Fact] public async Task NonPartial_Class_Ignored()

// Nested
[Fact] public async Task Nested_Property_Generates_AuxMethod()
[Fact] public async Task Cycle_Detection_Reports_FM0006()

// Constructor
[Fact] public async Task Record_Mapping_Uses_Constructor()
[Fact] public async Task Immutable_Class_Uses_ConstructorBinding()
[Fact] public async Task Hybrid_Type_Uses_Mixed_Strategy()

// Flatten
[Fact] public async Task Flat_Property_Maps_To_Nested_Path()
[Fact] public async Task Ambiguous_Flatten_Reports_FM0009()

// Profiles
[Fact] public async Task Api_Profile_Disables_Flatten()
[Fact] public async Task Domain_Profile_Enables_Constructor()
```

### 3. `FlowMapper.SnapshotTests`
Testes de snapshot usando `Verify.SourceGenerators`:

```csharp
[Fact] public Task BasicMapping_GeneratedCode_Snapshot()
[Fact] public Task NestedMapping_GeneratedCode_Snapshot()
[Fact] public Task ConstructorMapping_GeneratedCode_Snapshot()
[Fact] public Task FlattenMapping_GeneratedCode_Snapshot()
[Fact] public Task AllFeatures_Combined_Snapshot()
```

```csharp
// Exemplo:
public Task BasicMapping_Snapshot()
{
    var source = """
        [Map<User, UserDto>]
        public partial class UserMapper;

        public class User { public int Id { get; set; } public string Name { get; set; } }
        public class UserDto { public int Id { get; set; } public string Name { get; set; } }
        """;

    var generator = new FlowMapperGenerator();
    var driver = CSharpGeneratorDriver.Create(generator);

    var result = driver.RunGenerators(ParseSource(source));

    return Verifier.Verify(result);
}
```

### 4. `FlowMapper.IntegrationTests`
Testes de pipeline completo (geração + execução):

```csharp
[Fact] public void Generated_Mapper_Maps_Correctly()
{
    // Arrange
    var source = new User { Id = 1, Name = "Test" };
    var mapper = new UserMapper();

    // Act
    var result = mapper.Map(source);

    // Assert
    Assert.Equal(1, result.Id);
    Assert.Equal("Test", result.Name);
}
```

### 5. Utilities de Teste
- `MapperTestBase` — classe base com compilation helpers
- `SourceGeneratorTestHelper` — execução de generator em memória
- `AssertCode` — assertions de código gerado

## Critérios de Aceitação

- [ ] Todos os testes passam
- [ ] Cobertura de >80% no domínio core
- [ ] Snapshot tests detectam mudanças não intencionais
- [ ] Testes de geração cobrem todos os engines
- [ ] Testes de integração executam o mapper gerado

## Referências

- `Docs/Architecture/Estrutura-Soluction.md` — estrutura de diretórios de teste
- `Docs/Engines/3. DIAGNOSTICS ENGINE.md` — testes de diagnóstico (Microsoft.CodeAnalysis.Testing)
- `Docs/Engines/10. BENCHMARK ENGINE.md` — cenários de benchmark

## Dependências

- Sprints 01-10 — todos os componentes implementados
- NuGet: `xunit`, `FluentAssertions`, `Verify.SourceGenerators`, `Microsoft.CodeAnalysis.Testing`
