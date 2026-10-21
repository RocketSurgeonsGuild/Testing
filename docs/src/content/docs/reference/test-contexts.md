---
title: Test Contexts
description: The test context owns the Serilog logger and routes log output to the test framework.
sidebar:
    order: 1
tags:
    - logging
---

Every base class (`LoggerTest`, `AutoFakeTest`, `AutoSubstituteTest`) is generic over a **test context**: a class implementing `ILoggingTestContext`. The context builds the Serilog logger, streams every log event to observers (which is what `CaptureLogs` subscribes to), and decides where the output is written.

## Built-in contexts

| Context | Package | Writes logs to |
| --- | --- | --- |
| `TestRecord` | `Rocket.Surgery.Extensions.Testing.TUnit` | The current TUnit `TestContext` output. |
| `XUnitTestContext` | `Rocket.Surgery.Extensions.Testing.XUnit3` | The current xUnit v3 test output, via `ITestContextAccessor`. |
| `RocketSurgeryTestContext` | `Rocket.Surgery.Extensions.Testing` | Nowhere by default; configure sinks yourself. |

Both framework contexts accept a minimum `LogEventLevel` (default `Verbose`), an optional output template, and an optional `configureLogger` callback:

```csharp
public class QuietTests() : LoggerTest<TestRecord>(TestRecord.Create(LogEventLevel.Warning))
{
}
```

## Custom contexts

Derive from `RocketSurgeryTestContext<TContext>` (or from a framework context such as `TestRecord<TContext>`) when tests need shared state or extra logger configuration. Override `ConfigureLogger` to add enrichers, sinks or filters:

```csharp
public sealed class ApiTestContext() : TestRecord<ApiTestContext>()
{
    protected override void ConfigureLogger(ApiTestContext context, LoggerConfiguration loggerConfiguration)
    {
        base.ConfigureLogger(context, loggerConfiguration);
        loggerConfiguration.Enrich.WithProperty("Suite", "Api");
    }
}
```

## Global defaults

`RocketSurgeonsTestingDefaults` holds process-wide settings applied to every context:

- `OutputTemplate` — the default Serilog output template.
- `ConfigureLogging` — an optional callback run against every context's `LoggerConfiguration`, useful from a module initializer.

## Filtering by source context

`IncludeSourceContext` and `ExcludeSourceContext` (on the context, and forwarded by `LoggerTest`) filter events by their Serilog `SourceContext`. Use them to silence noisy framework loggers or to focus a test on one component.
