# Rocket Surgery Testing

Test helpers for .NET: a test base class that sends Serilog output to your test framework and captures log events for
assertions, auto-mocking containers for FakeItEasy and NSubstitute, test data builders, and a harness for testing Roslyn
source generators, analyzers and code fixes. TUnit and xUnit v3 are supported.

[![github-release-badge]][github-release]
[![github-license-badge]][github-license]
[![github-build-badge]][github-build]
[![codecov-badge]][codecov]
[![nuget-Rocket.Surgery.Extensions.Testing-badge]][nuget-Rocket.Surgery.Extensions.Testing]

Documentation: [docs/src/content/docs](https://github.com/RocketSurgeonsGuild/Testing/tree/main/docs/src/content/docs)

## Getting started

Add the package for your test framework. It brings in `Rocket.Surgery.Extensions.Testing`:

```bash
# TUnit
dotnet add package Rocket.Surgery.Extensions.Testing.TUnit

# xUnit v3
dotnet add package Rocket.Surgery.Extensions.Testing.XUnit3
```

Derive the test class from `LoggerTest<TContext>`. `Logger` writes to the current test's output, and `CaptureLogs`
collects the events written until it is disposed:

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

With `Rocket.Surgery.Extensions.Testing.FakeItEasy`, derive from `AutoFakeTest<TContext>` instead. `AutoFake.Resolve<T>()`
builds the class under test and fakes every dependency the container can't construct, returning the same fake each time
within a test:

```csharp
using FakeItEasy;
using Rocket.Surgery.Extensions.Testing;

public class CheckoutTests() : AutoFakeTest<TestRecord>(TestRecord.Create())
{
    [Test]
    public async Task Charges_the_customer()
    {
        var payments = AutoFake.Resolve<IPaymentGateway>();
        var checkout = AutoFake.Resolve<CheckoutService>();

        await checkout.CompleteAsync(orderId: 42);

        A.CallTo(() => payments.ChargeAsync(42, A<CancellationToken>._)).MustHaveHappenedOnceExactly();
    }
}
```

See the [quickstart](https://github.com/RocketSurgeonsGuild/Testing/blob/main/docs/src/content/docs/guide/quickstart.md), [auto-mocking](https://github.com/RocketSurgeonsGuild/Testing/blob/main/docs/src/content/docs/reference/auto-mocking.md),
[fixtures](https://github.com/RocketSurgeonsGuild/Testing/blob/main/docs/src/content/docs/reference/fixtures.md) and [source generator testing](https://github.com/RocketSurgeonsGuild/Testing/blob/main/docs/src/content/docs/reference/source-generators.md) pages.

## Packages

| Package                                                                                                        | NuGet                                                                                                                         | What it does                                                                                                                                    |
| -------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------- |
| [Rocket.Surgery.Extensions.Testing][nuget-Rocket.Surgery.Extensions.Testing]                                   | [![nuget-Rocket.Surgery.Extensions.Testing-badge]][nuget-Rocket.Surgery.Extensions.Testing]                                   | `LoggerTest<TContext>`, which wires Serilog to the test output and captures log events, and the DryIoc helpers the mocking packages build on    |
| [Rocket.Surgery.Extensions.Testing.TUnit][nuget-Rocket.Surgery.Extensions.Testing.TUnit]                       | [![nuget-Rocket.Surgery.Extensions.Testing.TUnit-badge]][nuget-Rocket.Surgery.Extensions.Testing.TUnit]                       | `TestRecord`, the test context for TUnit                                                                                                        |
| [Rocket.Surgery.Extensions.Testing.XUnit3][nuget-Rocket.Surgery.Extensions.Testing.XUnit3]                     | [![nuget-Rocket.Surgery.Extensions.Testing.XUnit3-badge]][nuget-Rocket.Surgery.Extensions.Testing.XUnit3]                     | `XUnitTestContext`, the test context for xUnit v3                                                                                               |
| [Rocket.Surgery.Extensions.Testing.FakeItEasy][nuget-Rocket.Surgery.Extensions.Testing.FakeItEasy]             | [![nuget-Rocket.Surgery.Extensions.Testing.FakeItEasy-badge]][nuget-Rocket.Surgery.Extensions.Testing.FakeItEasy]             | `AutoFakeTest<TContext>`: a DryIoc container that resolves the system under test and fakes missing dependencies with FakeItEasy                 |
| [Rocket.Surgery.Extensions.Testing.NSubstitute][nuget-Rocket.Surgery.Extensions.Testing.NSubstitute]           | [![nuget-Rocket.Surgery.Extensions.Testing.NSubstitute-badge]][nuget-Rocket.Surgery.Extensions.Testing.NSubstitute]           | `AutoSubstituteTest<TContext>`: the same container with NSubstitute substitutes                                                                 |
| [Rocket.Surgery.Extensions.Testing.Fixtures][nuget-Rocket.Surgery.Extensions.Testing.Fixtures]                 | [![nuget-Rocket.Surgery.Extensions.Testing.Fixtures-badge]][nuget-Rocket.Surgery.Extensions.Testing.Fixtures]                 | `ITestFixtureBuilder` and the `With` helpers for hand-written test data builders                                                                |
| [Rocket.Surgery.Extensions.Testing.AutoFixtures][nuget-Rocket.Surgery.Extensions.Testing.AutoFixtures]         | [![nuget-Rocket.Surgery.Extensions.Testing.AutoFixtures-badge]][nuget-Rocket.Surgery.Extensions.Testing.AutoFixtures]         | Source generator that writes a fixture builder from a constructor, marked with `[AutoFixture]`                                                  |
| [Rocket.Surgery.Extensions.Testing.SourceGenerators][nuget-Rocket.Surgery.Extensions.Testing.SourceGenerators] | [![nuget-Rocket.Surgery.Extensions.Testing.SourceGenerators-badge]][nuget-Rocket.Surgery.Extensions.Testing.SourceGenerators] | `GeneratorTestContextBuilder`: compiles sources, runs generators, analyzers, code fixes and refactorings, and snapshots the results with Verify |

`Rocket.Surgery.Extensions.Testing.Analyzers` comes in with `Rocket.Surgery.Extensions.Testing`, so you don't
reference it yourself. It ships no analyzer rules at present.

## Contributing

Run `mise install` before building: it installs the .NET SDK and the extra .NET 10 runtime that the net10.0 tests run on (`.config/mise.toml`). `global.json` pins the same SDK, so a `dotnet` outside mise fails fast unless that SDK is installed.

> [!NOTE]
> On macOS, the first `mise install` of a new .NET SDK can overwrite the shared `~/.local/share/mise/dotnet-root/dotnet` host in place, and macOS then kills every `dotnet` command (SIGKILL). Replace the host with a fresh copy to fix it:
>
> ```bash
> cd ~/.local/share/mise/dotnet-root && cp -p dotnet dotnet.fresh && mv -f dotnet.fresh dotnet
> ```

<!-- references -->

[github-release]: https://github.com/RocketSurgeonsGuild/Testing/releases/latest
[github-release-badge]: https://img.shields.io/github/release/RocketSurgeonsGuild/Testing.svg?logo=github&style=flat "Latest Release"
[github-license]: https://github.com/RocketSurgeonsGuild/Testing/blob/main/LICENSE
[github-license-badge]: https://img.shields.io/github/license/RocketSurgeonsGuild/Testing.svg?style=flat "License"
[github-build]: https://github.com/RocketSurgeonsGuild/Testing/actions/workflows/build.yml
[github-build-badge]: https://github.com/RocketSurgeonsGuild/Testing/actions/workflows/build.yml/badge.svg?branch=main "Build"
[codecov]: https://codecov.io/gh/RocketSurgeonsGuild/Testing
[codecov-badge]: https://img.shields.io/codecov/c/github/RocketSurgeonsGuild/Testing.svg?logo=codecov&style=flat "Code Coverage"
[nuget-Rocket.Surgery.Extensions.Testing]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing/
[nuget-Rocket.Surgery.Extensions.Testing-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.TUnit]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.TUnit/
[nuget-Rocket.Surgery.Extensions.Testing.TUnit-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.TUnit.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.XUnit3]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.XUnit3/
[nuget-Rocket.Surgery.Extensions.Testing.XUnit3-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.XUnit3.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.FakeItEasy]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.FakeItEasy/
[nuget-Rocket.Surgery.Extensions.Testing.FakeItEasy-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.FakeItEasy.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.NSubstitute]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.NSubstitute/
[nuget-Rocket.Surgery.Extensions.Testing.NSubstitute-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.NSubstitute.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.Fixtures]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.Fixtures/
[nuget-Rocket.Surgery.Extensions.Testing.Fixtures-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.Fixtures.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.AutoFixtures]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.AutoFixtures/
[nuget-Rocket.Surgery.Extensions.Testing.AutoFixtures-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.AutoFixtures.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.SourceGenerators]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.SourceGenerators/
[nuget-Rocket.Surgery.Extensions.Testing.SourceGenerators-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.SourceGenerators.svg?logo=nuget&style=flat
[nuget-Rocket.Surgery.Extensions.Testing.Analyzers]: https://www.nuget.org/packages/Rocket.Surgery.Extensions.Testing.Analyzers/
[nuget-Rocket.Surgery.Extensions.Testing.Analyzers-badge]: https://img.shields.io/nuget/v/Rocket.Surgery.Extensions.Testing.Analyzers.svg?logo=nuget&style=flat
