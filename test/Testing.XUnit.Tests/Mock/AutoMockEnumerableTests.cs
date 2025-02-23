using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Mock;

public class AutoMockEnumerableTests(ITestOutputHelper outputHelper) : AutoMockTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [Fact]
    public void Does_Not_Auto_Fake_Enumerable()
    {
        AutoMock.Provide<Item>(new A());
        AutoMock.Provide<Item>(new B());

        AutoMock.Resolve<IEnumerable<Item>>().Count().ShouldBe(2);
    }

    [Fact]
    public void Should_Handle_Creating_A_Mock_With_Logger()
    {
        var a = () =>
                {
                    var lt = AutoMock.Resolve<LoggerTest>();
                    AutoMock.Provide<Item>(lt);
                };
        a.ShouldNotThrow();
    }

    [Fact]
    public void Handle_Zero_Items()
    {
        AutoMock.Resolve<IEnumerable<Item>>().Count().ShouldBe(0);
    }

    [Fact]
    public void Handle_One_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(1);
        result.ShouldContain(fake1);
    }

    [Fact]
    public void Handle_Two_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(2);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
    }

    [Fact]
    public void Handle_Three_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(3);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
        result.ShouldContain(fake3);
    }

    [Fact]
    public void Handle_Four_Fake_Item()
    {
        var fake1 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());
        var fake4 = AutoMock.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoMock.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(4);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
        result.ShouldContain(fake3);
        result.ShouldContain(fake4);
    }

    public interface Item;

    private class A : Item;

    private class B : Item;

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
