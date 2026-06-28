### 📄 ADR-0002 — No Runtime Reflection

**Status:** Accepted

**Data:** 2026-06-27

----------

### Contexto

Reflection é uma ferramenta poderosa, porém possui impactos conhecidos:

-   maior consumo de CPU;
-   maior uso de memória;
-   incompatibilidades com cenários AOT;
-   menor previsibilidade de execução.

Como o FlowMapper possui acesso ao modelo semântico do Roslyn durante a compilação, Reflection deixa de ser necessária.

----------

### Decisão

O FlowMapper não utilizará Reflection para executar mapeamentos.

Reflection poderá existir apenas em componentes auxiliares que não participem da execução do mapper gerado (por exemplo, ferramentas de documentação ou CLI).

O código produzido pelo Source Generator deverá ser composto apenas por chamadas C# diretas.

----------

### Exemplo

Aceito

```csharp
return new UserDto
{
    Id = source.Id,
    Name = source.Name
};
```

Não aceito

```csharp
foreach(var property in typeof(User).GetProperties())
{
    ...
}
```

----------

### Consequências

-   Melhor desempenho.
-   Compatibilidade futura com Native AOT.
-   Código facilmente inspecionável.

----------
