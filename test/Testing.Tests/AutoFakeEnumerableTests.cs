using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Autofac.Extras.FakeItEasy;
using FluentAssertions;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoFakeEnumerableTests : AutoFakeTest
    {
        public AutoFakeEnumerableTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Information) { }

        [Fact]
        public void Test1()
        {
            AutoFake.Provide<Item>(new A());
            AutoFake.Provide<Item>(new B());

            AutoFake.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
        }

        public interface Item
        {

        }

        class A : Item
        {

        }

        class B : Item
        {

        }
    }
}
