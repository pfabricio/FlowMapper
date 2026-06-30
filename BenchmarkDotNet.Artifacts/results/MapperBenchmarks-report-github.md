```

BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.8655)
Unknown processor
.NET SDK 10.0.203
  [Host]     : .NET 10.0.7 (10.0.726.21808), X64 RyuJIT AVX2
  DefaultJob : .NET 10.0.7 (10.0.726.21808), X64 RyuJIT AVX2


```
| Method                 | Mean       | Error     | StdDev    | Gen0   | Allocated |
|----------------------- |-----------:|----------:|----------:|-------:|----------:|
| Manual_SimpleFlat      |   9.497 ns | 0.2460 ns | 0.2416 ns | 0.0096 |      40 B |
| FlowMapper_SimpleFlat  |  12.760 ns | 0.2375 ns | 0.2221 ns | 0.0096 |      40 B |
| AutoMapper_SimpleFlat  |  72.107 ns | 0.5495 ns | 0.4588 ns | 0.0095 |      40 B |
| Manual_Flatten         |  13.833 ns | 0.3106 ns | 0.2754 ns | 0.0115 |      48 B |
| FlowMapper_Flatten     |  12.949 ns | 0.3184 ns | 0.2823 ns | 0.0115 |      48 B |
| AutoMapper_Flatten     |  78.031 ns | 1.3023 ns | 1.2182 ns | 0.0114 |      48 B |
| Manual_Constructor     |   7.810 ns | 0.1643 ns | 0.1537 ns | 0.0115 |      48 B |
| FlowMapper_Constructor |   7.848 ns | 0.1672 ns | 0.1564 ns | 0.0115 |      48 B |
| AutoMapper_Constructor |  76.352 ns | 1.5952 ns | 3.3298 ns | 0.0114 |      48 B |
| Manual_Collection      |  44.024 ns | 0.3388 ns | 0.3004 ns | 0.0191 |      80 B |
| FlowMapper_Collection  |  43.477 ns | 0.3829 ns | 0.3198 ns | 0.0191 |      80 B |
| AutoMapper_Collection  | 722.719 ns | 6.1030 ns | 5.0963 ns | 0.0248 |     104 B |
