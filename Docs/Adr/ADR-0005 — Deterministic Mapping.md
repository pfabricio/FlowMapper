### 📄 ADR-0005 — Deterministic Mapping

**Status:** Accepted

**Data:** 2026-06-27

----------

### Contexto

Frameworks que tentam inferir automaticamente a intenção do desenvolvedor podem produzir resultados inesperados.

O objetivo do FlowMapper é ser previsível.

Se dois builds utilizarem exatamente o mesmo código-fonte, ambos deverão produzir exatamente o mesmo resultado.

----------

### Decisão

O FlowMapper seguirá o princípio da **Deterministic Mapping Generation**.

Toda decisão do gerador deverá ser reproduzível.

Não serão utilizados algoritmos baseados em heurísticas instáveis ou inferências não explícitas.

Quando houver mais de uma possibilidade de mapeamento, o gerador deverá emitir um diagnóstico em vez de escolher automaticamente.

----------

### Exemplos

Aceito

-   Um único caminho válido para uma propriedade.
-   Uma única estratégia de construtor compatível.
-   Convenções claramente definidas.

Não aceito

-   Escolher "a melhor" propriedade com base em similaridade.
-   Inferir intenção do desenvolvedor.
-   Alterar o resultado entre builds.

----------

### Consequências

-   Builds reproduzíveis.
-   Facilidade para testes de snapshot.
-   Diagnósticos mais claros.
-   Maior confiança no código gerado.

---
