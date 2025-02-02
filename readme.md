# Mapper

Basic .NET [expression](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/) based mapper library,
for experimenting with expressions and [benchmarking](https://github.com/dotnet/BenchmarkDotNet).

The library checks for changes before doing any mapping, and also indicates this to the library user.

## Usage

Usage is pretty simple, the `Mapper` class tries to be somewhat *fluent*:

```
var mapper = new Mapper<SourceClass, TargetClass>()
    .ForMember(target => target.SomeProp, source => source.SomeOtherProp)
    .ForMember(target => target.OtherProp, source => Helper.SecretComputation(source.Token))
    .ForMember(target => target.LastProp, source => source.LastProp)
    .Build();

var source = new SourceClass() { ... };
var target = new TargetClass();

// Map method returns true only if any value was changed
var changed = mapper.Map(source, target); 
```

Briefly:

- Create an instance using the desired type mappings
- Call `ForMember` to define the mappings you want included any number of times
- Call `Build` one time to finish initializing the instance
- Call `Map` to perform a mapping any number of times

## Limitations

Since `Mapper` uses expression trees there's alot of [limitations](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations) in the code that can be written inside the delegates. Some notable expression derived ones are:

- No null-coalescing/propagating operators
- No string interpolations
- No `ref`/`in`/`out` parameters
- No named/optional parameters

Other limitations:

- Regular equality (`==`) is used to determine if values are the same between source and target. An [IEqualityComparer<>](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.iequalitycomparer-1) can be passed for more advanced scenarios.
- The `ForMember` constrains types, but can get it wrong, notably when the target property can be implicitly cast to the source property. So if you want to map an `double` to a `float`, the library will allow this, but this will result in an runtime exception when calling `Build`.

## Benchmarks

Performance is measured against regular "hand-written" code which does the same thing as the mapper, by first checking for any difference in properties, and then updates if a difference is found.

    BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
    12th Gen Intel Core i5-1240P, 1 CPU, 16 logical and 12 physical cores
    .NET SDK 7.0.404
      [Host]     : .NET 7.0.14 (7.0.1423.51910), X64 RyuJIT AVX2
      DefaultJob : .NET 7.0.14 (7.0.1423.51910), X64 RyuJIT AVX2


| Method                        | Mean     | Error   | StdDev   |
|------------------------------ |---------:|--------:|---------:|
| Expression_Based_Mapping_Code | 448.4 ns | 8.89 ns | 12.46 ns |
| Manually_Written_Mapping_Code | 101.9 ns | 2.05 ns |  2.81 ns |