using DryIoc;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Fake;

public class AutoFakeTestTests(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{
    [Fact]
    public void Should_Create_Usable_Logger()
    {
        AutoFake.Resolve<Impl>();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_Logger()
    {
        var test = AutoFake.Resolve<LoggerImpl>();
        test.Write();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_LoggerFactory()
    {
        var test = AutoFake.Resolve<LoggerFactoryImpl>();
        test.Write();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Inject_GenericLogger()
    {
        var test = AutoFake.Resolve<GenericLoggerImpl>();
        test.Write();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    [Fact]
    public void Should_Provide_Values()
    {
        var item = AutoFake.Provide(new MyItem());
        ServiceProvider.GetRequiredService<MyItem>().Should().BeSameAs(item);
    }

    [Fact]
    public void Should_Return_Self_For_ServiceProvider()
    {
        ServiceProvider.GetRequiredService<IServiceProvider>().Should().Be(ServiceProvider);
    }

    [Fact]
    public void Should_Not_Fake_Optional_Parameters()
    {
        AutoFake.Resolve<Optional>().Item.Should().BeNull();
    }

    [Fact]
    public void Should_Populate_Optional_Parameters_When_Provided()
    {
        AutoFake.Provide<IItem>(new MyItem());
        AutoFake
           .Resolve<Optional>()
           .Item.Should()
           .NotBeNull()
           .And.Match(z => !FakeItEasy.Fake.IsFake(z));
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoFake.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        a.Should().Throw<TestBootstrapException>();
    }

    private class Impl : AutoFakeTest<TestOutputTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(Defaults.CreateTestOutput(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public IContainer Self => Container;

        protected override IContainer BuildContainer(IContainer container)
        {
            // invalid do not touch ServiceProvider or Container while constructing the container....
            return Container.GetRequiredService<IContainer>();
        }
    }

    private class LoggerImpl(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public void Write()
        {
            AutoFake.Resolve<ILogger>().LogError("abcd");
            AutoFake.Resolve<ILogger>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class LoggerFactoryImpl(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public void Write()
        {
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {Something}", "somevalue");
        }
    }

    public class GenericLoggerImpl(ITestOutputHelper outputHelper) : AutoFakeTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public void Write()
        {
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class MyItem : IItem { }

    public interface IItem { }

    private class Optional(IItem? item = null)
    {
        public IItem? Item { get; } = item;
    }
}
