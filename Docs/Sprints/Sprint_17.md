# Sprint 17 — Strongly Typed Callbacks (RFC-0002)

**Baseado em:** RFC-0002, ADR-0008, `Docs/Spec/rfc-0002-callbacks.md`

**Importância:** 🟡 Alta — melhoria de DX antes da v1.0

## Objetivo

Substituir `AfterMap(string)` e `ConstructUsing(string)` por overloads com `Expression<Action<...>>` / `Expression<Func<...>>`, permitindo lambdas inline e method groups com type safety.

---

## Tarefas

### 1. Core — Adicionar overloads Expression-based em `MappingExpression`

**Arquivo:** `src/FlowMapper.Core/MappingExpression.cs`

**O que fazer:**
1. Adicionar `using System.Linq.Expressions;`
2. Adicionar overload:
   ```csharp
   public MappingExpression<TSource, TDestination> AfterMap(
       Expression<Action<TSource, TDestination>> expression);
   ```
3. Adicionar overload:
   ```csharp
   public MappingExpression<TSource, TDestination> ConstructUsing(
       Expression<Func<TSource, TDestination>> expression);
   ```
4. Marcar overloads string como `[Obsolete]`
5. Implementação dos overloads Expression: extrair `.Body.ToString()` como fallback

### 2. Source Generator — Atualizar parsing de AfterMap/ConstructUsing

**Arquivo:** `src/FlowMapper.SourceGenerator/MappingCandidateFactory.cs`

**O que fazer:**
1. Criar `ExtractCallbackMethod(ExpressionSyntax, string callbackName)`:
   - `IdentifierNameSyntax` → nome do método (method group)
   - `MemberAccessExpressionSyntax` → `"Classe.Metodo"` (method group externo)
   - `SimpleLambdaExpressionSyntax` → corpo da lambda (inline)
   - `ParenthesizedLambdaExpressionSyntax` → corpo da lambda (inline)
2. Substituir extração atual (`call.Args[0].ToString().Trim('"')`) por `ExtractCallbackMethod()`
3. Ambos `AfterMap` e `ConstructUsing` usam o mesmo método

### 3. Source Generator — Atualizar Code Generator

**Arquivo:** `src/FlowMapper.SourceGenerator/Pipeline/Generator/FlowCodeGenerator.cs`

**O que fazer:**
1. Atualizar emissão de `AfterMap`:
   - Se for method group (identificador simples): `{Metodo}(source, target);`
   - Se for lambda inline (contém `=>`, `=`, etc): emitir corpo diretamente
2. Atualizar emissão de `ConstructUsing`:
   - Se for method group: `var target = {Metodo}(source);`
   - Se for lambda inline: emitir corpo diretamente

### 4. Abstractions — Manter `IMapper<,>` inalterado

**Nada muda aqui.** `IMapper`, `MapAttribute`, `FlowProfileAttribute` continuam iguais.

### 5. Samples — Atualizar para usar nova API

**Arquivos:**
- `samples/Profiles/Program.cs`
- `samples/BasicMapping/`
- `samples/ConstructorMapping/`

**O que fazer:**
1. Migrar `.AfterMap(nameof(X))` → `.AfterMap(X)` (method group)
2. Adicionar exemplos de lambda inline nos samples de Profile
3. Verificar compilação

### 6. Testes — Atualizar testes do Generator

**Arquivos:**
- `tests/FlowMapper.Generator.Tests/`
- `tests/FlowMapper.SnapshotTests/`

**O que fazer:**
1. Adicionar testes para:
   - `AfterMap` com method group
   - `AfterMap` com lambda inline
   - `ConstructUsing` com method group
   - `ConstructUsing` com lambda inline
2. Atualizar snapshots afetados
3. Verificar 0 regressão

### 7. Analyzers — Code fix para migração (opcional)

**Arquivo:** `src/FlowMapper.Analyzers/`

**O que fazer:**
- Se viável: criar code fix que converte `.AfterMap(nameof(X))` → `.AfterMap(X)`
- Prioridade: baixa (pode ficar para depois da v1.0)

---

## Critérios de Aceitação

- [ ] `dotnet build` — 0 erros, 0 warnings (excluindo `[Obsolete]` intencional)
- [ ] `dotnet test` — todos passando
- [ ] `AfterMap` aceita lambda inline e method group
- [ ] `ConstructUsing` aceita lambda inline e method group
- [ ] Código gerado correto para ambos os formatos
- [ ] Samples compilam com a nova API
- [ ] Overload string marcado como `[Obsolete]`

## Referências

- `Docs/Rfc/RFC-0002 — Strongly Typed Callbacks.md`
- `Docs/Adr/ADR-0008 — Strongly Typed Callbacks.md`
- `Docs/Spec/rfc-0002-callbacks.md`

## Dependências

- Sprint 16 — Public API Simplification (remover `[Map<,>]` evita conflito de mudanças)

---

