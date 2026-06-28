# Sprint 10 — Roslyn Analyzer (Camada 2)

**Baseado em:** `Docs/Engines/3. DIAGNOSTICS ENGINE.md` (seção 8 — Camada Avançada), `Docs/Architecture/Estrutura-Soluction.md`

## Objetivo

Implementar a camada de análise em tempo real (IDE-level) usando `DiagnosticAnalyzer`, fornecendo feedback instantâneo no editor com squiggles vermelhos antes mesmo da compilação.

---

## Tarefas

### 1. Criar Projeto `FlowMapper.Analyzers`
- `classlib` com dependências:
  - `Microsoft.CodeAnalysis.CSharp`
  - `Microsoft.CodeAnalysis.Analyzers`

### 2. `FlowMapperAnalyzer`
```csharp
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class FlowMapperAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics
        => ImmutableArray.Create(
            FlowDiagnostics.MissingDestinationProperty,
            FlowDiagnostics.TypeMismatch,
            FlowDiagnostics.InvalidMapper);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(AnalyzeSymbol, SymbolKind.NamedType);
    }
}
```

### 3. Análise de Símbolo — `AnalyzeSymbol`
```csharp
private void AnalyzeSymbol(SymbolAnalysisContext context)
{
    var symbol = (INamedTypeSymbol)context.Symbol;

    // Verificar se tem [Map<,>]
    var hasMapAttribute = symbol.GetAttributes()
        .Any(a => a.AttributeClass?.Name == "MapAttribute");

    if (!hasMapAttribute) return;

    // Validar:
    // 1. Classe é partial?
    if (!symbol.DeclaringSyntaxReferences.Any(IsPartial))
    {
        context.ReportDiagnostic(Diagnostic.Create(
            FlowDiagnostics.InvalidMapper,
            symbol.Locations.FirstOrDefault(),
            symbol.Name));
    }

    // 2. SourceType e DestinationType existem?
    // 3. Propriedades mapeadas (superficial)
}
```

### 4. Análise Adicional

| Verificação | Diagnóstico |
|---|---|
| Classe não partial | FM0003 — Invalid mapper |
| Atributo com tipos inválidos | FM0005 — Malformed attribute |
| Propriedade não encontrada | FM0001 — Not mapped |

### 5. CodeFixes (Opcional v1)
```csharp
[ExportCodeFixProvider(LanguageNames.CSharp)]
public class AddMappingCodeFix : CodeFixProvider
{
    // Sugestão: "Add missing mapping"
    // Sugestão: "Make class partial"
}
```

### 6. Configuração do `.csproj`
```xml
<EnforceExtendedAnalyzerRules>true</EnforceExtendedAnalyzerRules>
<IsRoslynComponent>true</IsRoslynComponent>
```

### 7. Diferenciação Generator vs Analyzer

| Característica | Generator (Sprint 02-03) | Analyzer |
|---|---|---|
| Momento | Build | Editor (tempo real) |
| Pipeline | Completo | Superficial |
| Diagnóstico | FM0001-FM0005 | Mesmos códigos |
| Performance | Tolerável | Crítico (síncrono) |

### 8. Testes
- Criar teste com `Microsoft.CodeAnalysis.Testing`:
  ```csharp
  await VerifyCS.VerifyAnalyzerAsync(...)
  ```

## Critérios de Aceitação

- [ ] Squiggles vermelhos aparecem no editor ao usar `[Map<,>]` em classe não partial
- [ ] Diagnósticos aparecem antes da compilação
- [ ] Analyzer não conflita com o Source Generator
- [ ] Performance aceitável no editor (sem travamentos)
- [ ] `SupportedDiagnostics` declarado corretamente

## Referências

- `Docs/Engines/3. DIAGNOSTICS ENGINE.md` seção 8 — camada avançada, FlowMapperAnalyzer
- `Docs/Architecture/Estrutura-Soluction.md` — namespace `Analyzers/Rules/`, `Analyzers/CodeFixes/`

## Dependências

- Sprint 03 — DiagnosticDescriptors compartilhados
- Sprint 02 — MapAttribute (símbolo a ser analisado)
