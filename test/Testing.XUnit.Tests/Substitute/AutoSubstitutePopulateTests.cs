using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Substitute;

public class AutoSubstitutePopulateTests(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [Fact]
    public void Should_Populate_Configuration_And_Services()
    {
        Container.Populate(new ServiceCollection().AddSingleton(new A()));
        Container.RegisterInstance<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["a"] = "1" }).Build());
        Configuration.GetValue<string>("a").ShouldBe("1");
        ServiceProvider.GetRequiredService<A>().ShouldBeSameAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public void Should_Populate_Services()
    {
        Populate(
            new ServiceCollection()
               .AddSingleton(new A())
               .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["a"] = "1" }).Build())
        );
        Configuration.GetValue<string>("a").ShouldBe("1");
        ServiceProvider.GetRequiredService<A>().ShouldBeSameAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public void Should_Populate_Container()
    {
        Populate(new Container());
        Configuration.GetValue<string>("a").ShouldBeNullOrEmpty();
        Container.IsRegistered<A>().ShouldBeFalse();
    }

    private class A;
}
