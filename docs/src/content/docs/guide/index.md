---
title: What is Rocket Surgery Testing?
description: An overview of the Rocket.Surgery.Extensions.Testing packages and which one to pick.
sidebar:
    order: 0
tags:
    - getting-started
---

Rocket Surgery Testing is a set of small libraries that remove the boilerplate from .NET unit tests:

- **Logging that shows up in your test output** and can be captured and asserted on.
- **Auto-mocking containers** so the system under test can be resolved without hand-writing every fake.
- **Fixture builders** for creating test data fluently, either hand-written or source generated.
- **A Roslyn test harness** for source generators, analyzers, code fixes, refactorings and completions.

## Packages

| Package | Use it for |
| --- | --- |
| `Rocket.Surgery.Extensions.Testing` | The core: `LoggerTest<TContext>`, `RocketSurgeryTestContext<TContext>` and Serilog log capture. |
| `Rocket.Surgery.Extensions.Testing.TUnit` | `TestRecord`, a test context that writes logs to TUnit's output. |
| `Rocket.Surgery.Extensions.Testing.XUnit3` | `XUnitTestContext`, a test context that writes logs to xUnit v3's output. |
| `Rocket.Surgery.Extensions.Testing.FakeItEasy` | `AutoFakeTest<TContext>` and `AutoFake`, auto-mocking with FakeItEasy. |
| `Rocket.Surgery.Extensions.Testing.NSubstitute` | `AutoSubstituteTest<TContext>` and `AutoSubstitute`, auto-mocking with NSubstitute. |
| `Rocket.Surgery.Extensions.Testing.Fixtures` | `ITestFixtureBuilder` and the `With(...)` helpers for hand-written builders. |
| `Rocket.Surgery.Extensions.Testing.AutoFixtures` | A source generator that writes fixture builders from `[AutoFixture]`. |
| `Rocket.Surgery.Extensions.Testing.SourceGenerators` | `GeneratorTestContextBuilder` and the Verify integration for Roslyn tests. |

Every test base class takes a **test context**. The context owns the Serilog logger and decides where log output goes, which is how the same base classes work across test frameworks: pick the framework package, then pass its context to the base class you want.

## Next Steps

- [Installation](./installation/) — add the packages for your test framework.
- [Quickstart](./quickstart/) — write a first test with captured logs.
