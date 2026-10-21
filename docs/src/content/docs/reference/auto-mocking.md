---
title: Auto-mocking
description: Resolve the system under test from a DryIoc container that fakes any dependency it can't build.
sidebar:
    order: 2
tags:
    - mocking
---

`AutoFakeTest<TContext>` (FakeItEasy) and `AutoSubstituteTest<TContext>` (NSubstitute) extend [`LoggerTest`](../../guide/quickstart/) with a DryIoc container. Any service the container can't construct is replaced by a fake, so you can resolve a class with a long constructor without writing every mock by hand.

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

`Resolve<T>()` returns the same fake every time within a test, so the instance you configure is the one injected into the system under test. The NSubstitute version is identical, with `AutoSubstitute` in place of `AutoFake`.

## Controlling registrations

| Member | Description |
| --- | --- |
| `AutoFake.Resolve<T>()` | Resolve a service, faking it if it isn't registered. |
| `AutoFake.Provide<TService>(instance)` | Register a real instance. |
| `AutoFake.Provide<TService, TImplementation>()` | Register an implementation type as a singleton. |
| `Populate(IServiceCollection)` | Copy registrations from a service collection into the container. |
| `Container` / `ServiceProvider` | The underlying DryIoc container and its `IServiceProvider`. |
| `Configuration` | The registered `IConfiguration`, or an empty one. |
| `BuildContainer(IContainer)` | Override to adjust the container before it is used. |

The container also registers an `ILoggerFactory` and `ILogger` wired to the test's Serilog logger, so code under test logs into the same test output and can be captured with `CaptureLogs`.

## Fake options

`AutoFakeTest` accepts an optional `Action<IFakeOptions>` that is applied to every fake it creates, for example to make fakes strict:

```csharp
public class StrictTests() : AutoFakeTest<TestRecord>(TestRecord.Create(), options => options.Strict())
{
}
```
