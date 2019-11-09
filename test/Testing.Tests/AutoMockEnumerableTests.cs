using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using FluentAssertions;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoMockEnumerableTests : AutoMockTest
    {
        public AutoMockEnumerableTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Information) { }

        [Fact]
        public void Test1()
        {
            AutoMock.Provide<Item>(new A());
            AutoMock.Provide<Item>(new B());

            AutoMock.Create<IEnumerable<Item>>().Should().HaveCount(2);
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
