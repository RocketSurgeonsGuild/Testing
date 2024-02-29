# Roslyn Source Generators Sample

A set of three projects that illustrates Roslyn source generators. Enjoy this template to learn from and modify source generators for your own needs.

## Content
### Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator
A .NET Standard project with implementations of sample source generators.
**You must build this project to see the result (generated code) in the IDE.**

- [SampleSourceGenerator.cs](SampleSourceGenerator.cs): A source generator that creates C# classes based on a text file (in this case, Domain Driven Design ubiquitous language registry).
- [SampleIncrementalSourceGenerator.cs](SampleIncrementalSourceGenerator.cs): A source generator that creates a custom report based on class properties. The target class should be annotated with the `Generators.ReportAttribute` attribute.

### Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator.Sample
A project that references source generators. Note the parameters of `ProjectReference` in [Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator.Sample.csproj](../Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator.Sample/Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator.Sample.csproj), they make sure that the project is referenced as a set of source generators. 

### Rocket.Surgery.Extensions.Testing.Fixtures.SourceGenerator.Tests
Unit tests for source generators. The easiest way to develop language-related features is to start with unit tests.

## How To?
### How to debug?
- Use the [launchSettings.json](Properties/launchSettings.json) profile.
- Debug tests.

### How can I determine which syntax nodes I should expect?
Consider installing the Roslyn syntax tree viewer plugin [Rossynt](https://plugins.jetbrains.com/plugin/16902-rossynt/).

### How to learn more about wiring source generators?
Watch the walkthrough video: [Let’s Build an Incremental Source Generator With Roslyn, by Stefan Pölz](https://youtu.be/azJm_Y2nbAI)
The complete set of information is available in [Source Generators Cookbook](https://github.com/dotnet/roslyn/blob/main/docs/features/source-generators.cookbook.md).


## Feature Requests

1. AutoFixture on the class being fixtured.  On the ViewModel not on a dummy class.
2. Where the files generate, file output path
3. Create the fixture in the same foldered/namespace where the class is decorated with the attribute
4. `[AutoMock]` public string Property { get; }
5. `[AutoMock]` decorate a class to use as the mock for an interface
    1. When you see the abstraction
6. Providing relevant mock values
7. Strict Fakes by default! Configurable.


## Todo

Global Configuration `[global:FixtureConfig(path = “foo”, Suffix = “Fixture”)]`
- file name convention
- file path to generate the source, maybe a different assembly
- Builder method name convention (Not Until Requested)

## MVP for Chase
- AutoFixture on the class, generate ClassNameFixture