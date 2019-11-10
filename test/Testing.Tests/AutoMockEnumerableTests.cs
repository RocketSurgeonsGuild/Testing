using System;
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
        public void Does_Not_Auto_Fake_Enumerable()
        {
            AutoMock.Provide<Item>(new A());
            AutoMock.Provide<Item>(new B());

            AutoMock.Create<IEnumerable<Item>>().Should().HaveCount(2);
        }

        [Fact]
        public void Should_Handle_Creating_A_Mock_With_Logger()
        {
            Action a = () =>
            {
                var lt = AutoMock.Create<LoggerTest>();
                AutoMock.Provide<Item>(lt);
            };
            a.Should().NotThrow();
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

        class LoggerTest : Item
        {
            public LoggerTest(ILogger logger)
            {

            }
        }
    }
}
