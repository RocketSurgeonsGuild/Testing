---
title: Fixtures
description: Fluent builders for test data, hand-written with ITestFixtureBuilder or generated with [AutoFixture].
sidebar:
    order: 3
tags:
    - fixtures
---

A fixture builder creates an object for a test with sensible defaults, and lets each test override only what it cares about.

## Hand-written builders

Implement the `ITestFixtureBuilder` marker interface and use the `With(ref field, value)` extension methods from `Rocket.Surgery.Extensions.Testing.Fixtures`. They assign the field and return the builder, which keeps each `WithX` method to one line. Overloads also append single items or ranges to `List<T>`, `Collection<T>` and dictionary fields.

```csharp
using Rocket.Surgery.Extensions.Testing.Fixtures;

public sealed class OrderBuilder : ITestFixtureBuilder
{
    private int _id = 1;
    private string _customer = "ada";
    private List<string>? _lines = [];

    public OrderBuilder WithId(int id) => this.With(ref _id, id);
    public OrderBuilder WithCustomer(string customer) => this.With(ref _customer, customer);
    public OrderBuilder WithLine(string line) => this.With(ref _lines, line);

    public Order Build() => new(_id, _customer, _lines ?? []);

    public static implicit operator Order(OrderBuilder builder) => builder.Build();
}
```

```csharp
Order order = new OrderBuilder().WithCustomer("grace").WithLine("widget");
```

## Generated builders with `[AutoFixture]`

`Rocket.Surgery.Extensions.Testing.AutoFixtures` is a source generator that writes the builder for you from the target type's constructor. Declare an empty partial class and point the attribute at the type:

```csharp
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

[AutoFixture(typeof(Authenticator))]
internal sealed partial class AuthenticatorFixture;
```

For a constructor `Authenticator(IAuthenticationClient client, ISecureStorage storage, ILogger<Authenticator> logger)` the generator emits:

- a `WithClient`, `WithStorage` and `WithLogger` method per constructor parameter;
- a default for each parameter, using a fake from the mocking library the test project references (`A.Fake<T>()` for FakeItEasy, `Substitute.For<T>()` for NSubstitute);
- an implicit conversion to `Authenticator`.

```csharp
Authenticator sut = new AuthenticatorFixture().WithStorage(storage);
```

The attribute can also be placed on the target class itself (`[AutoFixture]` with no type). The generator then emits `AuthenticatorFixture` in the target's namespace.

> [!NOTE]
> `params` array parameters are not supported.
