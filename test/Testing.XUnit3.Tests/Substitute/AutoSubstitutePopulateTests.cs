using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TUnit.Assertions.Extensions;
using TUnitAssert = TUnit.Assertions.Assert;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Substitute;

public class AutoSubstitutePopulateTests(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{
    [Fact]
    public async Task Should_Populate_Configuration_And_Services()
    {
        Container.Populate(new ServiceCollection().AddSingleton(new A()));
        Container.RegisterInstance<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["a"] = "1", }).Build());
        await Configuration.GetValue<string>("a").Should().BeEqualTo("1");
        await ServiceProvider.GetRequiredService<A>().Should().BeSameReferenceAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public async Task Should_Populate_Services()
    {
        Populate(
            new ServiceCollection()
               .AddSingleton(new A())
               .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["a"] = "1", }).Build())
        );
        await Configuration.GetValue<string>("a").Should().BeEqualTo("1");
        await ServiceProvider.GetRequiredService<A>().Should().BeSameReferenceAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public async Task Should_Populate_Container()
    {
        Populate(new Container());
        await TUnitAssert.That(Configuration.GetValue<string>("a")).IsNullOrEmpty();
        await Container.IsRegistered<A>().Should().BeFalse();
    }

    private class A { }
}
