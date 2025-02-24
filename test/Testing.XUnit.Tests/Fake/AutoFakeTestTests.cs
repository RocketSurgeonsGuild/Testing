using DryIoc;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Fake;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class AutoFakeTestTests(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
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
        _ = AutoFake.Resolve<Impl>();
        _ = A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_Logger()
    {
        var test = AutoFake.Resolve<LoggerImpl>();
        test.Write();
        _ = A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_LoggerFactory()
    {
        var test = AutoFake.Resolve<LoggerFactoryImpl>();
        test.Write();
        _ = A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_GenericLogger()
    {
        var test = AutoFake.Resolve<GenericLoggerImpl>();
        test.Write();
        _ = A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Provide_Values()
    {
        var item = AutoFake.Provide(new MyItem());
        ServiceProvider.GetRequiredService<MyItem>().ShouldBeSameAs(item);
    }

    [Fact]
    public void Should_Return_Self_For_ServiceProvider() => ServiceProvider.GetRequiredService<IServiceProvider>().ShouldBe(ServiceProvider);

    [Fact]
    public void Should_Not_Fake_Optional_Parameters() => AutoFake.Resolve<Optional>().Item.ShouldBeNull();

    [Fact]
    public void Should_Populate_Optional_Parameters_When_Provided()
    {
        _ = AutoFake.Provide<IItem>(new MyItem());
        var a = AutoFake
               .Resolve<Optional>()
               .Item.ShouldNotBeNull();
        FakeItEasy.Fake.IsFake(a).ShouldBe(false);
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoFake.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        _ = a.ShouldThrow<TestBootstrapException>();
    }

    private class Impl : AutoFakeTest<XUnitTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(XUnitDefaults.CreateTestContext(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public IContainer Self => Container;

        protected override IContainer BuildContainer(IContainer container) =>
            // invalid do not touch ServiceProvider or Container while constructing the container....
            Container.GetRequiredService<IContainer>();
    }

    private class LoggerImpl(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public void Write()
        {
            AutoFake.Resolve<ILogger>().LogError("abcd");
            AutoFake.Resolve<ILogger>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class LoggerFactoryImpl(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public void Write()
        {
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {Something}", "somevalue");
        }
    }

    [System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
    internal class GenericLoggerImpl(ITestOutputHelper outputHelper) : AutoFakeTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
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
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class MyItem : IItem;

    internal interface IItem;

    private class Optional(IItem? item = null)
    {
        public IItem? Item { get; } = item;
    }
}
