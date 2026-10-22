---
title: Installation
description: Add Rocket Surgery Testing to a TUnit or xUnit v3 test project.
sidebar:
    order: 1
tags:
    - getting-started
---

## Prerequisites

- .NET 10 SDK
- A test project using [TUnit](https://tunit.dev) or [xUnit v3](https://xunit.net)

## 1. Add the framework package

Pick the package that matches your test framework. It brings in the core `Rocket.Surgery.Extensions.Testing` package.

```bash
# TUnit
dotnet add package Rocket.Surgery.Extensions.Testing.TUnit

# xUnit v3
dotnet add package Rocket.Surgery.Extensions.Testing.XUnit3
```

## 2. Add a mocking package (optional)

```bash
# FakeItEasy
dotnet add package Rocket.Surgery.Extensions.Testing.FakeItEasy

# NSubstitute
dotnet add package Rocket.Surgery.Extensions.Testing.NSubstitute
```

## 3. Add fixture or generator tooling (optional)

```bash
# Hand-written fixture builders
dotnet add package Rocket.Surgery.Extensions.Testing.Fixtures

# Source-generated fixture builders
dotnet add package Rocket.Surgery.Extensions.Testing.AutoFixtures

# Testing Roslyn generators, analyzers and code fixes
dotnet add package Rocket.Surgery.Extensions.Testing.SourceGenerators
```

> [!TIP]
> If your solution uses Central Package Management, add each version to `Directory.Packages.props` and reference the packages without a `Version` attribute.
