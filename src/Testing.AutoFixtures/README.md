# Roslyn Source Generators Sample

A set of three projects that illustrates Roslyn source generators. Enjoy this template to learn from and modify source generators for your own needs.

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
8. Generate As<InterfaceName>() methods by looking at every interface it returns and providing a method that casts it as that interface type.


## Todo

Global Configuration `[global:FixtureConfig(path = “foo”, Suffix = “Fixture”)]`
- file name convention
- file path to generate the source, maybe a different assembly
- Builder method name convention (Not Until Requested)

## Unsupported
- param Type[]

## MVP for Chase
- AutoFixture on the class, generate ClassNameFixture
