using System.Globalization;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using Serilog;
using ILogger = Serilog.ILogger;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Fake;

public class AutoFakeEnumerableTests(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{

    [Fact]
    public void Does_Not_Auto_Fake_Enumerable()
    {
        AutoFake.Provide<Item>(new A());
        AutoFake.Provide<Item>(new B());

        AutoFake.Resolve<IEnumerable<Item>>().Should().HaveCount(2);
    }

    [Fact]
    public void Handle_Zero_Items()
    {
        AutoFake.Resolve<IEnumerable<Item>>().Should().HaveCount(0);
    }

    [Fact]
    public void Handle_One_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(1);
        result.Should().Contain(fake1);
    }

    [Fact]
    public void Handle_Two_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(2);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
    }

    [Fact]
    public void Handle_Three_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(3);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
        result.Should().Contain(fake3);
    }

    [Fact]
    public void Handle_Four_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake4 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        result.Should().HaveCount(4);
        result.Should().Contain(fake1);
        result.Should().Contain(fake2);
        result.Should().Contain(fake3);
        result.Should().Contain(fake4);
    }

    [Fact]
    [Obsolete("TBD")]
    public void Should_Handle_Creating_A_Mock_With_Logger()
    {
        var a = () =>
                {
                    var lt = AutoFake.Resolve<LoggerTest>();
                    AutoFake.Provide<Item>(lt);
                };
        a.Should().NotThrow();
    }

    public interface Item { }

    private class A : Item { }

    private class B : Item { }

    private class LoggerTest : Item
    {
        public LoggerTest(ILogger logger)
        {
            ArgumentNullException.ThrowIfNull(logger);
        }
    }
}
