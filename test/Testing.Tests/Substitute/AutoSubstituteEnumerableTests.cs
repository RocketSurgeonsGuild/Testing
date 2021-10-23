using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Substitute;

public class AutoSubstituteEnumerableTests : AutoSubstituteTest
{
    public AutoSubstituteEnumerableTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper, LogLevel.Information)
    {
    }

    [Fact]
    public void Does_Not_Auto_Substitute_Enumerable()
    {
        AutoSubstitute.Provide<Item>(new A());
        AutoSubstitute.Provide<Item>(new B());

        AutoSubstitute.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
    }

    [Fact]
    public void Handle_Zero_Items()
    {
        AutoSubstitute.Resolve<IEnumerable<Item>>().Should().HaveCount(0);
    }

    [Fact]
    public void Handle_One_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(1);
        result.Should().Contain(fake1);
    }

    [Fact]
    public void Handle_Two_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(2);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
    }

    [Fact]
    public void Handle_Three_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(3);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
        result.Should().Contain(fake3);
    }

    [Fact]
    public void Handle_Four_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake4 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
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
        public LoggerTest(ILogger logger)
        {
            if (logger == null)
            {
                throw new ArgumentNullException(nameof(logger));
            }
        }
    }
}
