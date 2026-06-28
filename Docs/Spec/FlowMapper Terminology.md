### 📖 FlowMapper Terminology

**Versão:** 1.0  
**Status:** Oficial  
**Última atualização:** 2026-06-27

----------

### 🎯 Objetivo

Este documento define a terminologia oficial utilizada pelo FlowMapper.

Todos os documentos, implementações, ADRs, PRDs e códigos-fonte devem utilizar os termos descritos aqui para manter consistência em toda a arquitetura.

Quando houver dúvidas sobre o significado de um termo, este documento é a referência oficial.

----------

### 🌊 Flow

### Definição

O **Flow** representa um mapeamento completo entre um tipo de origem e um tipo de destino.

Ele é a unidade central de trabalho do FlowMapper.

Todo o pipeline do framework existe para criar, validar, otimizar e transformar um Flow em código C#.

----------

### Exemplo

```
User
    │
    ▼
UserDto

```

Esse relacionamento representa um único Flow.

----------

### Responsabilidades

Um Flow contém informações como:

-   Tipo de origem
-   Tipo de destino
-   Propriedades mapeadas
-   Estratégias utilizadas
-   Configurações aplicadas
-   Diagnósticos associados   

----------

### 🌳 Flow Graph

### Definição

O **Flow Graph** é a representação interna utilizada pelo FlowMapper para organizar todas as informações necessárias para gerar um mapper.

Apesar do nome, ele **não representa um grafo matemático**.

O termo "Graph" foi adotado porque um mapeamento pode possuir relacionamentos entre diversos objetos, propriedades e regras.

Para o desenvolvedor, ele pode ser entendido simplesmente como o modelo interno do mapeamento.

----------

### Responsabilidades

O Flow Graph é utilizado para:

-   representar relacionamentos
-   validar regras
-   aplicar políticas
-   gerar código

----------

### 🏗 Flow Builder

### Definição

O **Flow Builder** é responsável por construir o Flow Graph.

Ele recebe uma definição de mapeamento e produz um modelo interno completo.

----------

### Responsabilidades

-   interpretar definições de mapeamento
-   resolver propriedades
-   identificar objetos aninhados
-   criar Property Flows
-   aplicar políticas iniciais

----------

### 📦 Mapper Definition

### Definição

Representa a definição descoberta pelo Source Generator antes da criação do Flow.

Ela contém apenas as informações necessárias para iniciar o processo de construção.

Exemplos:

-   tipo de origem
-   tipo de destino    
-   atributos
-   perfil   
-   opções
    
----------

### 🧩 Property Flow

### Definição

Representa o mapeamento de uma única propriedade.

----------

### Exemplo

```
User.Name
↓
UserDto.Name
```

Outro exemplo:

```
User.Address.City.Name
↓
UserDto.CityName
```

----------

### Informações

Um Property Flow conhece:

-   origem 
-   destino
-   estratégia
-   validações
-   conversões
    
----------

### 🌿 Flatten Flow

### Definição

Representa um caminho de propriedades reduzido para uma única propriedade de destino.

----------

### Exemplo

```
User.Address.City.Name
↓
CityName
```

----------

### 🌲 Nested Flow

### Definição

Representa um mapeamento entre objetos complexos.

----------

### Exemplo

```
User.Address
↓
AddressDto
```

Nesse caso um novo Flow poderá ser utilizado para realizar esse mapeamento.

----------

### 🏛 Constructor Binding

### Definição

Representa o relacionamento entre propriedades da origem e parâmetros de um construtor do destino.

----------

### Exemplo

```
User.Id
↓
UserDto(int id)
```

----------

### 🧭 Mapping Strategy

### Definição

Define como determinada propriedade será mapeada.

----------

### Exemplos

-   Direct Assignment
-   Constructor Mapping
-   Nested Mapping
-   Flatten Mapping
-   Custom Mapping
    

Cada Property Flow possui exatamente uma estratégia.

----------

### ⚙ Mapping Policy

### Definição

Representa regras globais ou específicas que influenciam o processo de geração.

----------

### Exemplos

-   Strict Mode
-   Warning Mode
-   Ignore Unmapped
-   Constructor Preference
-   Flatten Enabled

----------

### 👤 Profile

### Definição

Um Profile representa um conjunto de regras aplicadas a um grupo de mapeamentos.

----------

### Exemplos

```
Api
Domain
Integration
Persistence
```

Cada Flow pertence a exatamente um Profile.

----------

### ⚡ Flow Cache

### Definição

Armazena Flows previamente processados para evitar reconstruções desnecessárias.

O cache é baseado na assinatura do Flow.

----------

### 🔑 Flow Signature

### Definição

Identificador único de um Flow.

É utilizado para:

-   cache
-   comparação
-   reutilização
-   incremental generation

----------

### Exemplo conceitual

```
SourceType
+
DestinationType
+
Profile
+
Policies
```

----------

### 🔍 Validator

### Definição

Componente responsável por validar um Flow antes da geração de código.

----------

### Exemplos de validação

-   propriedades ausentes
-   ambiguidades
-   ciclos
-   construtores inválidos
-   flatten inválido
    

----------

### 🚨 Diagnostics

## Definição

Mensagens produzidas durante a validação.

Todos os diagnósticos seguem o padrão:

```
FM0001
FM0002
FM0003
```

Cada código representa um problema específico.

----------

### ⚙ Generator

### Definição

Transforma um Flow válido em código C#.

O Generator não toma decisões de negócio.

Sua única responsabilidade é converter o modelo interno em código-fonte.

----------

### 📝 Code Writer

### Definição

Componente responsável por escrever código C# de forma estruturada.

Ele abstrai detalhes como:

-   indentação 
-   namespaces
-   using
-   blocos
-   formatação

----------

### 📤 Flow Emitter

### Definição

Responsável por entregar o código gerado ao compilador.

No contexto do Source Generator, produz arquivos:

```
*.g.cs
```
----------

### 🔄 Pipeline

O pipeline oficial do FlowMapper é:

```
Roslyn
↓
Mapper Definition
↓
Flow Builder
↓
Flow Graph
↓
Validation
↓
Optimization
↓
Generator
↓
Flow Emitter
↓
Generated Mapper (.g.cs)
```

----------

### 📜 Princípios Arquiteturais

Toda implementação do FlowMapper deve respeitar os ADRs oficiais.

Especialmente:

-   Compile-Time First
-   No Runtime Reflection
-   Flow Graph
-   Source Generator Only
-   Deterministic Mapping
-   Explicit over Implicit

Esses princípios possuem prioridade sobre decisões de implementação.

----------

### 📚 Convenções de Nome

Os seguintes termos são considerados oficiais:

| Termo | Utilizar |
| :-- | :--: |
|Flow|✔|
|Flow Graph|✔|
|Flow Builder|✔|
|Property Flow|✔|
|Nested Flow|✔| 
|Flatten Flow|✔|
|Constructor Binding|✔|
|Flow Signature|✔|
|Mapping Strategy|✔
|Mapping Policy|✔|
|Flow Cache|✔|
|Flow Emitter|✔|

Novos componentes deverão seguir essa mesma convenção sempre que possível.

----------

### 🏁 Considerações Finais

O FlowMapper possui uma linguagem própria.

Essa terminologia não é apenas uma convenção de nomes, mas faz parte da identidade do projeto.

Toda nova funcionalidade deverá utilizar os conceitos definidos neste documento para preservar a consistência arquitetural do framework.

---
