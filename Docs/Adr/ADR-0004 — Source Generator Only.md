### 📄 ADR-0004 — Source Generator Only

**Status:** Accepted

**Data:** 2026-06-27

----------

### Contexto

Existem duas possibilidades para implementar um framework de mapeamento:

-   gerar código durante o runtime;
-   gerar código durante a compilação.

O FlowMapper opta exclusivamente pela segunda abordagem.

----------

### Decisão

Todo código de mapeamento deverá ser produzido pelo Source Generator.

Não existirão mecanismos de geração dinâmica em produção.

O runtime não poderá gerar novos mapeamentos.

----------

### Consequências

-   comportamento determinístico;
-   ausência de inicialização lenta ("warm-up");
-   menor consumo de memória;
-   maior previsibilidade.

----------

### Exceções

Ferramentas de desenvolvimento (CLI, documentação e benchmarks) podem utilizar componentes auxiliares sem afetar o runtime do framework.

----------
