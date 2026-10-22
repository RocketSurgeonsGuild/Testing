---
title: Quickstart
description: Write a first test that logs to the test output and asserts on captured log events.
sidebar:
    order: 2
tags:
    - getting-started
    - logging
---

This walks through a test class that derives from `LoggerTest<TContext>`, logs through Serilog, and asserts on the events it captured.

## TUnit

`TestRecord` is the TUnit test context. It writes every log event to the current test's output.

```csharp
using Serilog.Events;
using Rocket.Surgery.Extensions.Testing;

public class OrderServiceTests() : LoggerTest<TestRecord>(TestRecord.Create())
{
    [Test]
    public async Task Logs_a_warning_for_empty_orders()
    {
        using var capture = CaptureLogs(log => log.Level >= LogEventLevel.Warning, out var logs);

        Logger.Information("not captured: below the filter");
        Logger.Warning("order {OrderId} has no lines", 42);

        await Assert.That(logs.Count()).IsEqualTo(1);
    }
}
```

## xUnit v3

`XUnitTestContext` is the xUnit v3 test context. xUnit injects `ITestContextAccessor` into the test class constructor.

```csharp
using Rocket.Surgery.Extensions.Testing;
using Xunit;

public class OrderServiceTests(ITestContextAccessor accessor)
    : LoggerTest<XUnitTestContext>(new XUnitTestContext(accessor))
{
    [Fact]
    public void Logs_a_warning_for_empty_orders()
    {
        using var capture = CaptureLogs(out var logs);

        Logger.Warning("order {OrderId} has no lines", 42);

        Assert.Single(logs);
    }
}
```

## What you get from `LoggerTest`

| Member | Description |
| --- | --- |
| `Logger` | The Serilog `ILogger` for the test, already wired to the test output. |
| `CaptureLogs(out logs)` | Captures every log event written until the returned `IDisposable` is disposed. |
| `CaptureLogs(filter, out logs)` | Same, but only keeps events matching the filter. |
| `LogStream` | The raw `IObservable<LogEvent>` for custom subscriptions. |
| `CreateLoggerFactory()` | An `ILoggerFactory` that forwards `Microsoft.Extensions.Logging` loggers into the same Serilog pipeline. |
| `IncludeSourceContext` / `ExcludeSourceContext` | Limit output to, or hide, specific `SourceContext` values. |
| `Disposables` | A `CompositeDisposable` disposed with the test. |

## Next Steps

- [Auto-mocking](../reference/auto-mocking/) — resolve the system under test with its dependencies faked.
- [Fixtures](../reference/fixtures/) — build test data fluently.
