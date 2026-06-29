### 📄 ADR-0010 — Generator Decomposition

**Status:** Accepted

**Data:** 2026-06-29

**Baseado em:** RFC-0005

----------

### Contexto

O `FlowCodeGenerator` atualmente é uma classe estática de **205 linhas** com:

-   `Generate()` — orquestrador
-   `GenerateMapMethod()` — gera método `Map()` (~68 linhas)
-   `GenerateNestedMethod()` — gera método `MapXxx()` (~62 linhas)
-   `NormalizeExpression()` — utilitário
-   `GetNestedMethodName()` — utilitário

Problemas:
-   **Duplicação**: `GenerateMapMethod` e `GenerateNestedMethod` têm lógica quase idêntica de property assignment, constructor call e nested flow.
-   **Dificuldade de teste**: método `Generate()` retorna string completa — testar uma parte específica requer parsing.
-   **Extensibilidade**: adicionar uma nova estratégia de mapeamento significa editar dois métodos.
-   **Legibilidade**: 205 linhas com responsabilidades misturadas (namespace, class, usings, properties, constructors).

----------

### Decisão

**Decompor o `FlowCodeGenerator` em writers especializados.**

### Estrutura

```
Pipeline/Generator/
├── ICodeWriter.cs                 ← interface
├── UsingWriter.cs                 ← using statements
├── NamespaceWriter.cs             ← namespace declaration
├── ClassWriter.cs                 ← class declaration + IMapper
├── PropertyWriter.cs              ← property assignments (direct, flatten, expression)
├── ConstructorWriter.cs           ← constructor call + ConstructUsing
├── NestedWriter.cs                ← nested flow method calls
└── FlowCodeGenerator.cs           ← orquestrador reduzido
```

### Interface

```csharp
public interface ICodeWriter
{
    void Write(CodeWriterContext context, StringBuilder sb);
}
```

onde `CodeWriterContext` contém o `Flow` atual, o nome do mapper, e estado compartilhado.

----------

### Benefícios

-   **Fim da duplicação**: `PropertyWriter` e `ConstructorWriter` são reusados por Map e Nested.
-   **Testabilidade**: cada writer testado isoladamente com `StringBuilder` verificado.
-   **Extensibilidade**: nova estratégia = novo writer + registrar no orquestrador.
-   **Manutenção**: alterar formato de saída de uma seção não afeta as outras.

----------

### Custos

-   Mais arquivos (8 vs 1).
-   Indireção adicional (orquestrador + contexto).
-   Refatoração maior que as outras decomposições.

----------

### Nota

A implementação deve **preservar exatamente o mesmo output** do código gerado atualmente. Nenhuma mudança de comportamento.

---

