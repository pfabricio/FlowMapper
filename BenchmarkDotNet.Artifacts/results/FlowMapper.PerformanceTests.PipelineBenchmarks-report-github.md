```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.8457)
Unknown processor
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.726.21808), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.7 (10.0.726.21808), X64 RyuJIT AVX2


```
| Method                   | Mean      | Error     | StdDev    | Gen0   | Allocated |
|------------------------- |----------:|----------:|----------:|-------:|----------:|
| Build_BasicMapping       |  2.889 μs | 0.0556 μs | 0.0662 μs | 0.8049 |   3.29 KB |
| Build_ConstructorMapping |  3.641 μs | 0.0576 μs | 0.0511 μs | 1.3046 |   5.33 KB |
| Build_FlattenMapping     |  6.967 μs | 0.1334 μs | 0.1588 μs | 2.1973 |   9.02 KB |
| Build_NestedMapping      |  4.451 μs | 0.0724 μs | 0.0677 μs | 1.5717 |   6.44 KB |
| Pipeline_Basic           |  5.155 μs | 0.0882 μs | 0.0782 μs | 1.6708 |   6.84 KB |
| Pipeline_FourCandidates  | 27.965 μs | 0.4123 μs | 0.3857 μs | 8.6060 |  35.18 KB |
| Validate_Basic           |  4.399 μs | 0.0836 μs | 0.0741 μs | 1.3351 |   5.47 KB |
| Resolve_FlattenPath      |  1.654 μs | 0.0162 μs | 0.0152 μs | 0.5569 |   2.28 KB |
