using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Mock;

public class AutoMockEnumerableTests : AutoMockTest
{
    public AutoMockEnumerableTests(ITestOutputHelper outputHelper) : base(outputHelper, LogLevel.Information)
    {
    }

    [Fact]
    public void Does_Not_Auto_Fake_Enumerable()
    {
        AutoMock.Provide<Item>(new A());
        AutoMock.Provide<Item>(new B());

        AutoMock.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
    }

    [Fact]
    public void Should_Handle_Creating_A_Mock_With_Logger()
    {
        Action a = () =>
        {
            var lt = AutoMock.Resolve<LoggerTest>();
            AutoMock.Provide<Item>(lt);
        };
        a.Should().NotThrow();
    }

    [Fact]
    public void Handle_Zero_Items()
    {
        AutoMock.Resolve<IEnumerable<Item>>().Should().HaveCount(0);
    }

    [Fact]
    public void Handle_One_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(1);
        result.Should().Contain(fake1);
    }

    [Fact]
    public void Handle_Two_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(2);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
    }

    [Fact]
    public void Handle_Three_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(3);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
        result.Should().Contain(fake3);
    }

    [Fact]
    public void Handle_Four_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake4 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(4);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
        result.Should().Contain(fake3);
        result.Should().Contain(fake4);
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
