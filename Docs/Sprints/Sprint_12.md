# Sprint 12 — Benchmark Engine

**Baseado em:** `Docs/Engines/10. BENCHMARK ENGINE.md`, `Docs/Spec/SPEC.md` (seção 5.8)

## Objetivo

Criar o sistema de benchmarks comparando FlowMapper vs AutoMapper, com 5 cenários de teste, coleta de métricas e geração de relatório.

---

## Tarefas

### 1. Criar Projeto `FlowMapper.Benchmark.Tests`
- Console app com dependência `BenchmarkDotNet`

### 2. Modelos de Benchmark
```csharp
// Cenários
public class SimpleMappingBenchmark
{
    private IMapper _autoMapper;
    private UserMapper _flowMapper;
    private User _user;

    [GlobalSetup]
    public void Setup()
    {
        // Configurar AutoMapper
        var config = new MapperConfiguration(cfg => cfg.CreateMap<User, UserDto>());
        _autoMapper = config.CreateMapper();

        // FlowMapper (código gerado)
        _flowMapper = new UserMapper();

        _user = new User { Id = 1, Name = "Paulo" };
    }

    [Benchmark(Baseline = true)]
    public UserDto AutoMapper() => _autoMapper.Map<UserDto>(_user);

    [Benchmark]
    public UserDto FlowMapper() => _flowMapper.Map(_user);
}
```

### 3. Cenários de Teste
| # | Cenário | Descrição |
|---|---|---|
| 1 | Simple Mapping | `User → UserDto` (flat, 3 props) |
| 2 | Nested Mapping | `User → UserDto` com `Address → City` |
| 3 | Constructor Mapping | Record mapping |
| 4 | Large Object Graph | 15 propriedades aninhadas |
| 5 | Stress Test | 100k iterações |

### 4. `BenchmarkResult`
```csharp
public class BenchmarkResult
{
    public string Scenario { get; set; }
    public double FlowMapperTimeMs { get; set; }
    public double AutoMapperTimeMs { get; set; }
    public long FlowMapperAllocations { get; set; }
    public long AutoMapperAllocations { get; set; }
    public double SpeedRatio { get; set; }
    public string Winner { get; set; }
}
```

### 5. `BenchmarkReportBuilder`
```csharp
public static class BenchmarkReportBuilder
{
    public static string GenerateMarkdown(BenchmarkResult result);
    public static string GenerateJson(BenchmarkResult result);
    public static void ExportToFile(BenchmarkResult result, string outputPath);
}
```

### 6. `BenchmarkRunnerService`
```csharp
public class BenchmarkRunnerService
{
    public void RunAll();
    public BenchmarkResult RunScenario<T>() where T : class;
}
```

### 7. Métricas Coletadas
BenchmarkDotNet fornece automaticamente:
- Mean time
- Allocated memory
- Gen 0/1/2 GC collections
- Standard deviation
- Throughput (ops/sec)

### 8. Critérios de Comparação Justa
- FlowMapper: runtime execution apenas (custo de build excluído)
- AutoMapper: runtime mapping cost
- Mesmo modelo, mesmas propriedades, mesmos dados

## Critérios de Aceitação

- [ ] 5 cenários de benchmark executáveis
- [ ] Comparação FlowMapper vs AutoMapper
- [ ] Relatório gerado em Markdown
- [ ] Métricas de tempo e alocação coletadas
- [ ] Resultados exportáveis (JSON/Markdown)

## Referências

- `Docs/Engines/10. BENCHMARK ENGINE.md` — cenários, métricas, report builder
- `Docs/Spec/SPEC.md` seção 5.8 — benchmark engine

## Dependências

- Sprint 11 — mappers gerados disponíveis para benchmark
- NuGet: `BenchmarkDotNet`, `AutoMapper` (apenas para benchmark)
