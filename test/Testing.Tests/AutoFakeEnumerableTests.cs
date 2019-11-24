using System;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using System.Collections.Generic;
using Autofac.Extras.FakeItEasy;
using FluentAssertions;
using JetBrains.Annotations;

#pragma warning disable CA1034 // Nested types should not be visible
#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1040 // Avoid empty interfaces

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
        [Obsolete("TBD")]
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
