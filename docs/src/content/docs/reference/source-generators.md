---
title: Source Generator Testing
description: Compile sources, run Roslyn generators, analyzers and code fixes, and snapshot the results with Verify.
sidebar:
    order: 4
tags:
    - source-generator
---

`Rocket.Surgery.Extensions.Testing.SourceGenerators` builds an in-memory compilation, runs the Roslyn components you register against it, and returns the results in a shape that [Verify](https://github.com/VerifyTests/Verify) can snapshot.

## Building a context

`GeneratorTestContextBuilder` is an immutable builder: each `With*` / `Add*` call returns a new builder, so a shared base configuration can be branched per test.

```csharp
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

public class MyGeneratorTests() : LoggerTest<TestRecord>(TestRecord.Create())
{
    [Test]
    public async Task Generates_partial_members()
    {
        var context = GeneratorTestContextBuilder
                     .Create()
                     .WithLogger(Logger)
                     .WithGenerator<MyIncrementalGenerator>()
                     .AddReferences(typeof(MyAttribute))
                     .AddSources("""
                         [My] public partial class Widget { }
                         """)
                     .Build();

        await Verify(context.GenerateAsync());
    }
}
```

| Method | Description |
| --- | --- |
| `WithGenerator<T>()` | Run an `ISourceGenerator` or `IIncrementalGenerator`. |
| `WithAnalyzer<T>()` | Run a `DiagnosticAnalyzer`. |
| `WithCodeFix<T>()` | Run a `CodeFixProvider` against the reported diagnostics. |
| `AddSources(...)` | Add source files, as strings or `SourceText`. |
| `AddReferences(...)` | Add metadata references by type, assembly, assembly name or `MetadataReference`. |
| `WithProjectName`, `WithLanguageVersion`, `WithDocumentationMode`, `WithSourceCodeKind` | Shape the compilation. |
| `WithFeature`, `AddPreprocessorSymbol` | Parse options for features and `#if` symbols. |
| `WithAssemblyLoadContext` | Load generator outputs into a specific context (a collectible one is used by default). |
| `WithCustomizer` | Apply a `GeneratorTestResultsCustomizer` to this test's snapshot. |

`GenerateAsync()` returns `GeneratorTestResults`: the final compilation, every generated source, diagnostics, and the results of any analyzers, code fixes, refactorings and completions that were registered.

## Verify integration

Call `VerifyGeneratorTextContext.Initialize` once, from a module initializer, to register the converters that turn `GeneratorTestResults` into readable snapshots:

```csharp
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() =>
        VerifyGeneratorTextContext.Initialize(
            DiagnosticSeverity.Warning,
            Customizers.IncludeInputs,
            Customizers.IncludeParseOptions,
            Customizers.IncludeReferences
        );
}
```

The diagnostic severity filters which diagnostics land in the snapshot. `Customizers` controls what else is included (inputs, parse options, global and file options, references, the context id) and can be overridden per test with `WithCustomizer`.
