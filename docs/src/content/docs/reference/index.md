---
title: Reference
description: How the test contexts, auto-mocking base classes, fixture builders and generator harness fit together.
sidebar:
    order: 0
tags:
    - api
---

These pages explain each building block and how to use it. For every public type and member, see the [API Reference](../api/), which is generated from the XML docs.

- [Test Contexts](./test-contexts/) — `RocketSurgeryTestContext`, `TestRecord` (TUnit) and `XUnitTestContext` (xUnit v3), and how to customize logging.
- [Auto-mocking](./auto-mocking/) — `AutoFakeTest` / `AutoSubstituteTest` and their DryIoc container.
- [Fixtures](./fixtures/) — hand-written builders with `ITestFixtureBuilder`, and generated builders with `[AutoFixture]`.
- [Source Generator Testing](./source-generators/) — `GeneratorTestContextBuilder` and the Verify integration.
