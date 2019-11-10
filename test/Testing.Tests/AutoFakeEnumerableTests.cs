using System;
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
        public void Does_Not_Auto_Fake_Enumerable()
        {
            Fake.Provide<Item>(new A());
            Fake.Provide<Item>(new B());

            Fake.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
        }

        [Fact]
        public void Should_Handle_Creating_A_Mock_With_Logger()
        {
            Action a = () =>
            {
                var lt = AutoFake.Create<LoggerTest>();
                AutoFake.Provide<Item>(lt);
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
