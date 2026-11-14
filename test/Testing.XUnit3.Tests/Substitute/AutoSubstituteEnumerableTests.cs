using Microsoft.Extensions.Logging;
using TUnit.Assertions.Extensions;
using TUnitAssert = TUnit.Assertions.Assert;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Substitute;

public class AutoSubstituteEnumerableTests(ITestOutputHelper testOutputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(testOutputHelper))
{
    [Fact]
    public async Task Does_Not_Auto_Substitute_Enumerable()
    {
        AutoSubstitute.Provide<Item>(new A());
        AutoSubstitute.Provide<Item>(new B());

        await AutoSubstitute.Resolve<IEnumerable<Item>>().Count().Should().BeEqualTo(2);
    }

    [Fact]
    public async Task Handle_Zero_Items() => await AutoSubstitute.Resolve<IEnumerable<Item>>().Count().Should().BeEqualTo(0);

    [Fact]
    public async Task Handle_One_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(1);
        await result.Should().Contain(fake1);
    }

    [Fact]
    public async Task Handle_Two_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(2);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
    }

    [Fact]
    public async Task Handle_Three_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(3);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
        await result.Should().Contain(fake3);
    }

    [Fact]
    public async Task Handle_Four_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake4 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        await result.Count().Should().BeEqualTo(4);
        await result.Should().Contain(fake1);
        await result.Should().Contain(fake2);
        await result.Should().Contain(fake3);
        await result.Should().Contain(fake4);
    }

    [Fact(Skip = "Obsolete?")]
    [Obsolete("TBD")]
    public async Task Should_Handle_Creating_A_Substitute_With_Logger()
    {
        void a()
        {
            var lt = AutoSubstitute.Resolve<LoggerTest>();
            AutoSubstitute.Provide<Item>(lt);
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
