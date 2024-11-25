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

Since `Mapper` uses expression trees there's alot of [limitations](https://learn.microsoft.com/en-us/dotnet/csharp/advanced-topics/expression-trees/#limitations) in the code that can be written inside the delegates. The most notable expression derived ones are:

- No null-coalescing/propagating operators
- No string interpolations
- No `ref`/`in`/`out` parameters
- No named/optional parameters

Other limitations:

- Regular in-equality (`!=`) is used to determine if values are different between source and target.