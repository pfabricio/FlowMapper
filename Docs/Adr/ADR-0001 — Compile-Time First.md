### 📄 ADR-0001 — Compile-Time First

**Status:** Accepted

**Data:** 2026-06-27

----------

### Contexto

Frameworks tradicionais de mapeamento de objetos executam grande parte da resolução dos mapeamentos durante o runtime, utilizando Reflection, Expression Trees ou geração dinâmica de delegates.

Embora essa abordagem ofereça flexibilidade, ela introduz custos em tempo de execução, dificulta a detecção antecipada de erros e reduz a previsibilidade do comportamento da aplicação.

O FlowMapper nasce com uma proposta diferente.

Toda decisão possível deve ser tomada durante a compilação.

----------

### Decisão

O FlowMapper adota o princípio **Compile-Time First**.

Sempre que uma informação puder ser determinada durante o build, ela deverá ser resolvida pelo Source Generator.

O código produzido deverá ser equivalente ao que um desenvolvedor experiente escreveria manualmente.

----------

### Consequências

### Benefícios

-   Zero reflexão durante o runtime.
-   Erros detectados durante a compilação.
-   Código totalmente otimizado pelo compilador.
-   Facilidade para depuração.
-   Compatibilidade com Native AOT.

----------

### Custos

-   Aumento do trabalho realizado durante o build.
-   Implementação mais complexa do Source Generator.
-   Maior responsabilidade do pipeline de geração.

----------

### Princípios derivados

-   Nunca mover lógica do compile-time para o runtime sem justificativa arquitetural.
-   Preferir geração explícita de código.
-   O runtime deve apenas executar código previamente gerado.

----------
