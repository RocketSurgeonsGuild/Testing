using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoFakePopulateTests : AutoFakeTest
    {
        public AutoFakePopulateTests(ITestOutputHelper outputHelper) : base(outputHelper) => Populate(
            new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>() { ["a"] = "1" }).Build(),
            new ServiceCollection().AddSingleton(new A())
        );

        [Fact]
        public void Should_Populate_Services()
        {
            Configuration.GetValue<string>("a").Should().Be("1");
            Container.Resolve<A>().Should().BeSameAs(ServiceProvider.GetService<A>());
        }

        class A { }
    }
}
