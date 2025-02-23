using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Substitute;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class AutoSubstituteEnumerableTests(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Fact]
    public void Does_Not_Auto_Substitute_Enumerable()
    {
        _ = AutoSubstitute.Provide<Item>(new A());
        _ = AutoSubstitute.Provide<Item>(new B());

        AutoSubstitute.Resolve<IEnumerable<Item>>().Count().ShouldBe(2);
    }

    [Fact]
    public void Handle_Zero_Items() => AutoSubstitute.Resolve<IEnumerable<Item>>().Count().ShouldBe(0);

    [Fact]
    public void Handle_One_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(1);
        result.ShouldContain(fake1);
    }

    [Fact]
    public void Handle_Two_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(2);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
    }

    [Fact]
    public void Handle_Three_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(3);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
        result.ShouldContain(fake3);
    }

    [Fact]
    public void Handle_Four_Substitute_Item()
    {
        var fake1 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake2 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake3 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());
        var fake4 = AutoSubstitute.Provide(NSubstitute.Substitute.For<Item>());

        var result = AutoSubstitute.Resolve<IEnumerable<Item>>().ToArray();
        result.Count().ShouldBe(4);
        result.ShouldContain(fake1);
        result.ShouldContain(fake2);
        result.ShouldContain(fake3);
        result.ShouldContain(fake4);
    }

    [Fact]
    [Obsolete("TBD")]
    public void Should_Handle_Creating_A_Substitute_With_Logger()
    {
        var a = () =>
                {
                    var lt = AutoSubstitute.Resolve<LoggerTest>();
                    _ = AutoSubstitute.Provide<Item>(lt);
                };
        a.ShouldNotThrow();
    }

    internal interface Item;

    private class A : Item;

    private class B : Item;

    private class LoggerTest : Item
    {
        public LoggerTest(ILogger logger)
        {
            if (logger is not null)
            {
                return;
            }

            throw new ArgumentNullException(nameof(logger));
        }
    }
}
