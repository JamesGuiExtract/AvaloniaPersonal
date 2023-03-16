``` ini

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22621.1413/22H2/2022Update/SunValley2)
12th Gen Intel Core i7-12700, 1 CPU, 20 logical and 12 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT AVX2


```
|                    Method |     Mean |    Error |   StdDev |
|-------------------------- |---------:|---------:|---------:|
| QueryEnvironmentByContext | 97.20 ms | 1.773 ms | 2.914 ms |
|     QueryEnvironmentByKey | 84.33 ms | 1.669 ms | 2.393 ms |
|     QueryUnresolvedAlerts | 59.35 ms | 0.547 ms | 0.456 ms |
|            QueryAlertById | 31.87 ms | 0.593 ms | 0.555 ms |
