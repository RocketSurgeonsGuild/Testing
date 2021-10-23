using DryIoc;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Fake;

public class AutoFakeTestTests : AutoFakeTest
{
    public AutoFakeTestTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    private class Impl : AutoFakeTest
    {
        public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Logger.LogError("abcd");
            Logger.LogError("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess : AutoFakeTest
    {
        public DoubleAccess(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        protected override IContainer BuildContainer(IContainer container)
        {
            // invalid do not touch ServiceProvider or Container while constructing the container....
            return Container.GetRequiredService<IContainer>();
        }

        public IContainer Self => Container;
    }

    [Fact]
    public void Should_Create_Usable_Logger()
    {
        AutoFake.Resolve<Impl>();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    private class LoggerImpl : AutoFakeTest
    {
        public LoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public void Write()
        {
            AutoFake.Resolve<ILogger>().LogError("abcd");
            AutoFake.Resolve<ILogger>().LogError("abcd {Something}", "somevalue");
        }
    }

    [Fact]
    public void Should_Inject_Logger()
    {
        var test = AutoFake.Resolve<LoggerImpl>();
        test.Write();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    private class LoggerFactoryImpl : AutoFakeTest
    {
        public LoggerFactoryImpl(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public void Write()
        {
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
            AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {Something}", "somevalue");
        }
    }

    [Fact]
    public void Should_Inject_LoggerFactory()
    {
        var test = AutoFake.Resolve<LoggerFactoryImpl>();
        test.Write();
        A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
    }

    public class GenericLoggerImpl : AutoFakeTest
    {
        public GenericLoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper)
        {
        }

        public void Write()
        {
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
            AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {Something}", "somevalue");
        }
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
        AutoFake.Resolve<Optional>().Item.Should().NotBeNull()
                .And.Match(z => !FakeItEasy.Fake.IsFake(z));
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoFake.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        a.Should().Throw<ApplicationException>();
    }

    private class MyItem : IItem
    {
    }

    public interface IItem
    {
    }

    private class Optional
    {
        public IItem? Item { get; }

        public Optional(IItem? item = null)
        {
            Item = item;
        }
    }
}
