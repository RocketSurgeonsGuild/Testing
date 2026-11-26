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

| Method                                                                                                              | Description                                                                                                                                         |
| ------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |
| `WithGenerator<T>()`                                                                                                | Run an `ISourceGenerator` or `IIncrementalGenerator`.                                                                                               |
| `WithAnalyzer<T>()`                                                                                                 | Run a `DiagnosticAnalyzer`.                                                                                                                         |
| `WithCodeFix<T>()`                                                                                                  | Run a `CodeFixProvider` against the reported diagnostics.                                                                                           |
| `AddSources(...)`                                                                                                   | Add source files, as strings or `SourceText`.                                                                                                       |
| `AddReferences(...)`                                                                                                | Add metadata references by type, assembly, assembly name or `MetadataReference`.                                                                    |
| `WithProjectName`, `WithLanguageVersion`, `WithDocumentationMode`, `WithSourceCodeKind`                             | Shape the compilation.                                                                                                                              |
| `WithFeature`, `AddPreprocessorSymbol`, `WithParseOptions`                                                          | Parse options for features and `#if` symbols. Every parse-option setter keeps the earlier ones, and setting a feature key again replaces its value. |
| `WithNullableContext`, `WithAllowUnsafe`, `WithOptimizationLevel`, `WithDiagnosticOption`, `WithCompilationOptions` | Compilation options (see below).                                                                                                                    |
| `WithIncrementalTracking`, `WithDriverOptions`                                                                      | Generator driver options, for incremental caching tests (see below).                                                                                |
| `WithAssemblyLoadContext`                                                                                           | Load generator outputs into a specific context (a collectible one is used by default).                                                              |
| `WithCustomizer`                                                                                                    | Apply a `GeneratorTestResultsCustomizer` to this test's snapshot.                                                                                   |

`WithGenerator`, `WithAnalyzer`, `WithCodeFix` and `WithCodeRefactoring` throw an `ArgumentException` when the type is not of that kind; such types used to be dropped silently.

`GenerateAsync()` returns `GeneratorTestResults`: the final compilation, every generated source, diagnostics, and the results of any analyzers, code fixes, refactorings and completions that were registered.

## Compilation options

The compilation starts from `new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)`. Change it with the shortcuts, or with `WithCompilationOptions` for anything else:

```csharp
var results = await GeneratorTestContextBuilder
                   .Create()
                   .WithNullableContext(NullableContextOptions.Enable)
                   .WithAllowUnsafe()
                   .WithOptimizationLevel(OptimizationLevel.Release)
                   .WithDiagnosticOption("CS0168", ReportDiagnostic.Error)
                   .WithCompilationOptions(options => options.WithOverflowChecks(true))
                   .AddSources(source)
                   .GenerateAsync();
```

Severity options work as they do in an `.editorconfig` or `.globalconfig`: `AddGlobalOption("dotnet_diagnostic.KRX030.severity", "none")` applies to the whole compilation and `AddOption("Handlers.cs", "dotnet_diagnostic.KRX030.severity", "error")` to one file. The values are `default`, `error`, `warning`, `suggestion`, `silent` and `none`, and they change compiler, generator and analyzer diagnostics alike. A per-file `generated_code = true` marks that file as generated. Category keys (`dotnet_analyzer_diagnostic.category-*.severity`) are not mapped.

## Incremental generator tests

Every generator runs in its own driver, and each `GeneratorTestResult` keeps that driver (`Driver`) and its run result (`DriverRunResult`, `RunResult`). Turn on step tracking with `WithIncrementalTracking()` and run the same drivers again with `RerunAsync`:

```csharp
var first = await GeneratorTestContextBuilder
                 .Create()
                 .WithIncrementalTracking()
                 .WithGenerator<MyIncrementalGenerator>()
                 .AddSources(source)
                 .GenerateAsync();

var second = await first.RerunAsync(compilation => compilation.AddSyntaxTrees(
    CSharpSyntaxTree.ParseText("public class Unrelated { }", first.ParseOptions)));

var steps = second.Results[typeof(MyIncrementalGenerator)].RunResult!.Value.TrackedSteps["MyStep"];
await steps.SelectMany(z => z.Outputs)
           .All(z => z.Reason is IncrementalStepRunReason.Cached or IncrementalStepRunReason.Unchanged)
           .Should().BeTrue();
```

The edit gets the input compilation, so it can add or replace syntax trees or swap a reference (`compilation.ReplaceReference(old, @new)`). A second callback edits each driver (for example `driver.ReplaceAdditionalText(...)`). A rerun runs the analyzers and emits again, but does not load the assembly (the first run already loaded one with that name) and does not run code fixes or refactorings again.

## Emit and assertions

`EmitResult`, `EmitDiagnostics` and `Image` (the PE bytes) say what happened when the final compilation was emitted. When emit fails, `Assembly` and `MetadataReference` are null, and `AddCompilationReferences` throws an exception that names the failed result and lists its emit errors.

`EnsureDiagnosticSeverity`, `AssertCompilationWasSuccessful` and `AssertGenerationWasSuccessful` list every failing diagnostic in the exception message. The parameterless assertions still fail on warnings; pass `DiagnosticSeverity.Error` to allow them. `AssertCompilationWasSuccessful` also fails when the final compilation did not emit.

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

The diagnostic severity filters which diagnostics land in the snapshot; `null` (what `Initialize(customizers)` passes) includes them all. `Customizers` controls what else is included (inputs, parse options, global and file options, references, the context id) and can be overridden per test with `WithCustomizer`.
