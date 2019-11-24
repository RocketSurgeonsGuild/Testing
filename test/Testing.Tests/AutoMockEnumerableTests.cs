using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using FluentAssertions;
using JetBrains.Annotations;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1040 // Avoid empty interfaces

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

        private class A : Item
        {

        }

        private class B : Item
        {

        }

        private class LoggerTest : Item
        {
#pragma warning disable IDE0060 // Remove unused parameter
#pragma warning disable RCS1163 // Unused parameter.
#pragma warning disable CS0436 // Type conflicts with imported type
            public LoggerTest([NotNull] ILogger logger)
#pragma warning restore CS0436 // Type conflicts with imported type
#pragma warning restore RCS1163 // Unused parameter.
#pragma warning restore IDE0060 // Remove unused parameter
            {
                if (logger == null)
                {
                    throw new ArgumentNullException(nameof(logger));
                }
            }
        }
    }
}
