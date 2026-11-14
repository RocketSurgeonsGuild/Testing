using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Arg = NSubstitute.Arg;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests.Substitute;

public class AutoSubstituteTestTests(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
{
    [Fact]
    public void Should_Create_Usable_Logger()
    {
        AutoSubstitute.Resolve<Impl>();
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
        var testOutputHelper = AutoSubstitute.Resolve<ITestOutputHelper>();
        testOutputHelper.Received().WriteLine(Arg.Any<string>());
    }

    [Fact]
    public async Task Should_Provide_Values()
    {
        var item = AutoSubstitute.Provide(new MyItem());
        await ServiceProvider.GetRequiredService<MyItem>().Should().BeSameReferenceAs(item);
    }

    [Fact]
    public async Task Should_Return_Self_For_ServiceProvider() => await ServiceProvider.GetRequiredService<IServiceProvider>().Should().BeEqualTo(ServiceProvider);

    [Fact]
    public async Task Should_Not_Fake_Optional_Parameters() => await AutoSubstitute.Resolve<Optional>().Item.Should().BeNull();

    [Fact]
    public async Task Should_Populate_Optional_Parameters_When_Provided()
    {
        AutoSubstitute.Provide<IItem>(new MyItem());
        await AutoSubstitute
           .Resolve<Optional>()
           .Item
           .Should().NotBeNull();
    }

    [Fact]
    public async Task Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoSubstitute.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        await a.Should().Throw<TestBootstrapException>();
    }

    private class Impl : AutoSubstituteTest<TestOutputTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(Defaults.CreateTestOutput(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public IContainer Self => Container;

        protected override IContainer BuildContainer(IContainer container) =>
            // invalid do not touch ServiceProvider or Container while constructing the container....
            Container.GetRequiredService<IContainer>();
    }

    private class LoggerImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public void Write()
        {
            AutoSubstitute.Resolve<ILogger>().LogError("abcd");
            AutoSubstitute.Resolve<ILogger>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class LoggerFactoryImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        public void Write()
        {
            AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
            AutoSubstitute.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {Something}", "somevalue");
        }
    }

    public class GenericLoggerImpl(ITestOutputHelper outputHelper) : AutoSubstituteTest<TestOutputTestContext>(Defaults.CreateTestOutput(outputHelper))
    {
        private readonly ITestOutputHelper _otherHelper = outputHelper;

        public void Write()
        {
            AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
            AutoSubstitute.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {Something}", "somevalue");
        }
    }

    private class MyItem : IItem { }

    public interface IItem { }

    private class Optional(IItem? item = null)
    {
        public IItem? Item { get; } = item;
    }
}
