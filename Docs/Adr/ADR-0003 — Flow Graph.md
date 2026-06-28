### 📄 ADR-0003 — Flow Graph

**Status:** Accepted

**Data:** 2026-06-27

----------

### Contexto

O principal objetivo do FlowMapper não é copiar propriedades.

Seu objetivo é representar uma transformação entre dois modelos.

Essa transformação pode conter:

-   propriedades simples;
-   objetos aninhados;
-   construtores;
-   flatten;
-   regras de perfil.

Todos esses elementos possuem relações entre si.

----------

### Decisão

Internamente o FlowMapper representará um mapeamento como um **Flow Graph**.

Esse grafo será o modelo central do framework.

Nenhum componente deverá gerar código diretamente a partir do Roslyn.

Todos deverão consumir o Flow Graph.

----------

### Arquitetura

```
Roslyn
    ↓
Flow Graph
    ↓
Validation
    ↓
Optimization
    ↓
Code Generation
```

----------

### Benefícios

-   Separação clara de responsabilidades.
-   Pipeline extensível.
-   Facilidade para novos recursos.
-   Reutilização entre diferentes engines.

----------

### Consequências

Novas funcionalidades deverão atuar sobre o Flow Graph.

Nunca diretamente sobre o código gerado.

----------
