### 📄 ADR-0006 — Explicit over Implicit

**Status:** Accepted

**Data:** 2026-06-27

**Princípio:** Fundamental

----------

### Contexto

Frameworks de mapeamento costumam utilizar convenções e heurísticas para inferir automaticamente a intenção do desenvolvedor.

Embora essa abordagem reduza a quantidade de configuração inicial, ela pode produzir comportamentos inesperados quando existem múltiplas possibilidades válidas.

Exemplos comuns incluem:

-   seleção automática de propriedades por similaridade de nome;
-   escolha automática entre múltiplos construtores;
-   resolução implícita de caminhos de propriedades;
-   conversões automáticas não configuradas;
-   flatten baseado em heurísticas.

Esses comportamentos tornam o sistema menos previsível e dificultam sua compreensão.

O objetivo do FlowMapper é oferecer um comportamento consistente, transparente e reproduzível.

----------

### Problema

Considere o seguinte modelo:

```csharp
public class User
{
    public string Name { get; set; }

    public string FullName { get; set; }
}

public class UserDto
{
    public string Name { get; set; }
}
```

Existem duas propriedades potencialmente compatíveis.

Qual delas deveria ser utilizada?

Uma biblioteca baseada em heurísticas poderia escolher:

-   Name
-   FullName
-   a maior similaridade
-   a primeira encontrada
-   outra regra interna

Nenhuma dessas decisões é explicitamente conhecida pelo desenvolvedor.

----------

### Decisão

O FlowMapper adota o princípio:

> **Quando existir mais de uma interpretação válida, o framework não decidirá pelo desenvolvedor.**

Nesses casos o FlowMapper deverá:

-   emitir um diagnóstico;
-   explicar o motivo;
-   solicitar configuração explícita.

----------

### Princípios

### 1. Convenções são permitidas

Convenções simples e determinísticas são desejáveis.

Exemplo:

```
Id → Id
Name → Name
CreatedAt → CreatedAt
```

Estas convenções são estáveis e facilmente compreendidas.

----------

### 2. Heurísticas não são permitidas

O FlowMapper não utilizará algoritmos que tentem descobrir a intenção do usuário.

Exemplos proibidos:

-   similaridade de nomes;
-   distância de Levenshtein;
-   prefixos ou sufixos implícitos;
-   escolha do "melhor candidato";
-   algoritmos probabilísticos.

----------

### 3. Ambiguidade sempre gera diagnóstico

Sempre que houver duas ou mais soluções possíveis, o build deverá informar o problema.

Exemplo:

```
FM0020
Multiple source members can map to destination property 'Name'.
Explicit configuration is required.
```

----------

### 4. Configuração explícita sempre vence

Quando o desenvolvedor fornecer uma configuração explícita, ela terá prioridade sobre qualquer convenção.

Exemplo conceitual:

```csharp
[MapProperty(nameof(User.FullName), nameof(UserDto.Name))]
```

ou

```csharp
cfg.ForMember(
    d => d.Name,
    s => s.FullName);
```

Independentemente do formato que a API venha a adotar no futuro, a intenção do desenvolvedor deve prevalecer.

----------

### 5. Convenção é apenas um ponto de partida

O papel das convenções é reduzir trabalho repetitivo.

Nunca substituir decisões de negócio.

----------

### Benefícios

-   comportamento previsível;
-   geração determinística;
-   facilidade de depuração;
-   menor número de erros ocultos;
-   código gerado facilmente auditável.

----------

### Custos

O usuário poderá escrever mais configurações em cenários complexos.

Esse custo é considerado aceitável em troca da previsibilidade do framework.

----------

### Exemplos

### Aceito

Mapeamento direto:

```
User.Name
↓
UserDto.Name
```

Resultado:

```
Name = source.Name;
```

----------

### Aceito

Mapeamento explícito:

```
FullName↓Name
```

Configurado pelo usuário.

----------

### Não aceito

Escolher automaticamente:

```
NameouFullName
```

----------

### Não aceito

Inferir que:

```
CustomerName
```

significa

```
Customer.Name
```

sem uma convenção ou configuração explícita.

----------

### Consequências arquiteturais

Todos os componentes do FlowMapper deverão respeitar este princípio.

Isso inclui:

-   FlowBuilder
-   Nested Engine
-   Flatten Engine
-   Constructor Engine
-   Profile Engine
-   Code Generator
-   Diagnostics Engine

Nenhum componente poderá implementar heurísticas ocultas para resolver ambiguidades.

----------

### Relação com outros ADRs

Este ADR complementa e reforça:

-   **ADR-0001 — Compile-Time First**
-   **ADR-0003 — Flow Graph**
-   **ADR-0005 — Deterministic Mapping**

Em caso de conflito entre conveniência e previsibilidade, a previsibilidade deve prevalecer.

----------

### Regra de Ouro do FlowMapper

> **"Se o compilador não puder determinar uma única resposta correta, o FlowMapper não fará uma escolha. Ele pedirá ao desenvolvedor que a faça."**

---
