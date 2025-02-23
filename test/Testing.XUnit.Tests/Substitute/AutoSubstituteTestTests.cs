using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit.Abstractions;
using Arg = NSubstitute.Arg;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Substitute;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class AutoSubstituteTestTests(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
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
    public void Should_Create_Usable_Logger()
    {
        _ = AutoSubstitute.Resolve<Impl>();
        AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
    }

    [Fact]
    public void Should_Inject_Logger()
    {
        var test = AutoSubstitute.Resolve<LoggerImpl>();
        test.Write();
        AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
    }

    [Fact]
    public void Should_Inject_LoggerFactory()
    {
        var test = AutoSubstitute.Resolve<LoggerFactoryImpl>();
        test.Write();
        AutoSubstitute.Resolve<ITestOutputHelper>().Received().WriteLine(Arg.Any<string>());
    }

    [Fact]
    public void Should_Inject_GenericLogger()
    {
        var test = AutoSubstitute.Resolve<GenericLoggerImpl>();
        test.Write();
        var outputHelper = AutoSubstitute.Resolve<ITestOutputHelper>();
        outputHelper.Received().WriteLine(Arg.Any<string>());
    }

    [Fact]
    public void Should_Provide_Values()
    {
        var item = AutoSubstitute.Provide(new MyItem());
        ServiceProvider.GetRequiredService<MyItem>().ShouldBeSameAs(item);
    }

    [Fact]
    public void Should_Return_Self_For_ServiceProvider() => ServiceProvider.GetRequiredService<IServiceProvider>().ShouldBe(ServiceProvider);

    [Fact]
    public void Should_Not_Fake_Optional_Parameters() => AutoSubstitute.Resolve<Optional>().Item.ShouldBeNull();

    [Fact]
    public void Should_Populate_Optional_Parameters_When_Provided()
    {
        _ = AutoSubstitute.Provide<IItem>(new MyItem());
        _ = AutoSubstitute
           .Resolve<Optional>()
           .Item
           .ShouldNotBeNull();
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoSubstitute.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        _ = a.ShouldThrow<TestBootstrapException>();
    }

    private class Impl : AutoSubstituteTest<XUnitTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(XUnitDefaults.CreateTestContext(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public IContainer Self => Container;

        protected override IContainer BuildContainer(IContainer container) =>
            // invalid do not touch ServiceProvider or Container while constructing the container....
            Container.GetRequiredService<IContainer>();
    }

    private class LoggerImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public void Write()
        {
            AutoSubstitute.Resolve<ILogger>().LogError("abcd");
            AutoSubstitute.Resolve<ILogger>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class LoggerFactoryImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public void Write()
        {
            AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
            AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {Something}", "somevalue");
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class GenericLoggerImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
        private string DebuggerDisplay
        {
            get
            {
                return ToString();
            }
        }

        public void Write()
        {
            AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
            AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class MyItem : IItem;

    internal interface IItem;

    private class Optional(IItem? item = null)
    {
        public IItem? Item { get; } = item;
    }
}
