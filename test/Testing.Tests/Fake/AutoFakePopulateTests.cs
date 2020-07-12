using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using DryIoc;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoFakePopulateTests : AutoFakeTest
    {
        public AutoFakePopulateTests(ITestOutputHelper outputHelper) : base(outputHelper)  {}

        [Fact]
        public void Should_Populate_Configuration_And_Services()
        {
            Populate(
                new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>() { ["a"] = "1" }).Build(),
                new ServiceCollection().AddSingleton(new A())
            );
            Configuration.GetValue<string>("a").Should().Be("1");
            ServiceProvider.GetRequiredService<A>().Should().BeSameAs(ServiceProvider.GetService<A>());
        }

        [Fact]
        public void Should_Populate_Services()
        {
            Populate(
                new ServiceCollection()
                   .AddSingleton(new A())
                   .AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>() { ["a"] = "1" }).Build())
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

        class A { }
    }
}
