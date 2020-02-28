using System;
using System.Collections.Generic;
using System.Linq;
using FakeItEasy;
using FluentAssertions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoSubstituteEnumerableTests : AutoSubstituteTest
    {
        public AutoSubstituteEnumerableTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, LogLevel.Information) { }

        [Fact]
        public void Does_Not_Auto_Substitute_Enumerable()
        {
            NSubstitute.Provide<Item>(new A());
            NSubstitute.Provide<Item>(new B());

            NSubstitute.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
        }

        [Fact]
        public void Handle_Zero_Items()
        {
            NSubstitute.Resolve<IEnumerable<Item>>().Should().HaveCount(0);
        }

        [Fact]
        public void Handle_One_Substitute_Item()
        {
            var fake1 = NSubstitute.Provide(Substitute.For<Item>());

            var result = NSubstitute.Resolve<IEnumerable<Item>>().ToArray();
            result.Should().HaveCount(1);
            result.Should().Contain(fake1);
        }

        [Fact]
        public void Handle_Two_Substitute_Item()
        {
            var fake1 = NSubstitute.Provide(Substitute.For<Item>());
            var fake2 = NSubstitute.Provide(Substitute.For<Item>());

            var result = NSubstitute.Resolve<IEnumerable<Item>>().ToArray();
            result.Should().HaveCount(2);
            result.Should().Contain(fake1);
            result.Should().Contain(fake2);
        }

        [Fact]
        public void Handle_Three_Substitute_Item()
        {
            var fake1 = NSubstitute.Provide(Substitute.For<Item>());
            var fake2 = NSubstitute.Provide(Substitute.For<Item>());
            var fake3 = NSubstitute.Provide(Substitute.For<Item>());

            var result = NSubstitute.Resolve<IEnumerable<Item>>().ToArray();
            result.Should().HaveCount(3);
            result.Should().Contain(fake1);
            result.Should().Contain(fake2);
            result.Should().Contain(fake3);
        }

        [Fact]
        public void Handle_Four_Substitute_Item()
        {
            var fake1 = NSubstitute.Provide(Substitute.For<Item>());
            var fake2 = NSubstitute.Provide(Substitute.For<Item>());
            var fake3 = NSubstitute.Provide(Substitute.For<Item>());
            var fake4 = NSubstitute.Provide(Substitute.For<Item>());

            var result = NSubstitute.Resolve<IEnumerable<Item>>().ToArray();
            result.Should().HaveCount(4);
            result.Should().Contain(fake1);
            result.Should().Contain(fake2);
            result.Should().Contain(fake3);
            result.Should().Contain(fake4);
        }

        [Fact(Skip = "Obsolete?")]
        [Obsolete("TBD")]
        public void Should_Handle_Creating_A_Substitute_With_Logger()
        {
            Action a = () =>
            {
                var lt = AutoSubstitute.Resolve<LoggerTest>();
                AutoSubstitute.Provide<Item>(lt);
            };
            a.Should().NotThrow();
        }

        public interface Item { }

        private class A : Item { }

        private class B : Item { }

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