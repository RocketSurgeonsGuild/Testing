using TUnit.Assertions.Extensions;
using ILogger = Serilog.ILogger;
using TUnitAssert = TUnit.Assertions.Assert;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Fake;

public class AutoFakeEnumerableTests(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{

    [Fact]
    public async Task Does_Not_Auto_Fake_Enumerable()
    {
        AutoFake.Provide<Item>(new A());
        AutoFake.Provide<Item>(new B());

        await AutoFake.Resolve<IEnumerable<Item>>().Count().Should().BeEqualTo(2);
    }

    [Fact]
    public async Task Handle_Zero_Items() => await AutoFake.Resolve<IEnumerable<Item>>().Count().Should().BeEqualTo(0);

    [Fact]
    public async Task Handle_One_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(1);
        await result.Should().Contain(fake1);
    }

    [Fact]
    public async Task Handle_Two_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(2);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
    }

    [Fact]
    public async Task Handle_Three_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(3);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
        await result.Should().Contain(fake3);
    }

    [Fact]
    public async Task Handle_Four_Fake_Item()
    {
        var fake1 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake2 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake3 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());
        var fake4 = AutoFake.Provide(FakeItEasy.A.Fake<Item>());

        var result = AutoFake.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(4);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
        await result.Should().Contain(fake3);
        await result.Should().Contain(fake4);
    }

    [Fact]
    [Obsolete("TBD")]
    public async Task Should_Handle_Creating_A_Mock_With_Logger()
    {
        void a()
        {
            var lt = AutoFake.Resolve<LoggerTest>();
            AutoFake.Provide<Item>(lt);
        }
        await TUnitAssert.That(a).ThrowsNothing();
    }

    public interface Item { }

    private class A : Item { }

    private class B : Item { }

    private class LoggerTest : Item
    {
        public LoggerTest(ILogger logger) => ArgumentNullException.ThrowIfNull(logger);
    }
}
