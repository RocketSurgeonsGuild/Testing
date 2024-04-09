# Auto Fixture Generator

I know there are other opinions about this.  I want to preface this with, I am not a fan of auto mocking frameworks.  This came out of a necessity for trying to adhere to that.
So in the end, I likely should have evaluated other options, but here I am.

## Opinionated

ReactiveUI had a pattern that I used from a long time ago to write test fixtures.  I appreciated the simple builder pattern with extension methods.  AFter a time, I realized I was writing a lot of mocking code.  I wrote live templates for JetBrains Rider to speed up the process.  Still, the more I added, the more aggravating the maintenance got.
This library seeks to reduce the maintenance burden by auto generating the builder methods for the consumer.


## Feature Requests

1. [x] AutoFixture on the class being fixtured.  On the ViewModel not on a dummy class.
2. [ ] Where the files generate, file output path
3. [ ] Create the fixture in the same foldered/namespace where the class is decorated with the attribute
4. [ ] `[AutoMock]` public string Property { get; }
5. [ ] `[AutoMock]` decorate a class to use as the mock for an interface
    1. When you see the abstraction
6. [ ] Providing relevant mock values
7. [ ] Strict Fakes by default! Configurable.
8. [ ] Generate As<InterfaceName>() methods by looking at every interface it returns and providing a method that casts it as that interface type.

## Research

Global Configuration `[global:FixtureConfig(path = “foo”, Suffix = “Fixture”)]`
- file name convention
- file path to generate the source, maybe a different assembly
- Builder method name convention (Not Until Requested)

## Unsupported
- `param Type[]`
