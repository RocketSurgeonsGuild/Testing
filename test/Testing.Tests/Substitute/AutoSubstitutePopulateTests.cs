using DryIoc;
using DryIoc.Microsoft.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Substitute;

public class AutoSubstitutePopulateTests : AutoSubstituteTest
{
    [Fact]
    public void Should_Populate_Configuration_And_Services()
    {
        Container.Populate(new ServiceCollection().AddSingleton(new A()));
        Container.RegisterInstance<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["a"] = "1", }).Build());
        Configuration.GetValue<string>("a").Should().Be("1");
        ServiceProvider.GetRequiredService<A>().Should().BeSameAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public void Should_Populate_Services()
    {
        Populate(
            new ServiceCollection()
               .AddSingleton(new A())
               .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string> { ["a"] = "1", }).Build())
        );
        Configuration.GetValue<string>("a").Should().Be("1");
        ServiceProvider.GetRequiredService<A>().Should().BeSameAs(ServiceProvider.GetService<A>());
    }

    [Fact]
    public void Should_Populate_Container()
    {
        Populate(new Container());
        Configuration.GetValue<string>("a").Should().BeNullOrEmpty();
        Container.IsRegistered<A>().Should().BeFalse();
    }

    public AutoSubstitutePopulateTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

    private class A { }
}
