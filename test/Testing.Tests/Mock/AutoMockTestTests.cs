using DryIoc;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests.Mock;

public class AutoMockTestTests : AutoMockTest
{
    public AutoMockTestTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    private class Impl : AutoMockTest
    {
        public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            Logger.LogError("abcd");
            Logger.LogError("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess : AutoMockTest
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
        AutoMock.Resolve<Impl>();
        AutoMock.Mock<ITestOutputHelper>().Verify(x => x.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Should_Provide_Values()
    {
        var item = AutoMock.Provide(new MyItem());
        ServiceProvider.GetRequiredService<MyItem>().Should().BeSameAs(item);
    }

    [Fact]
    public void Should_Return_Self_For_ServiceProvider()
    {
        ServiceProvider.GetRequiredService<IServiceProvider>().Should().Be(ServiceProvider);
    }

    [Fact]
    public void Should_Not_Mock_Optional_Parameters()
    {
        AutoMock.Resolve<Optional>().Item.Should().BeNull();
    }

    [Fact]
    public void Should_Populate_Optional_Parameters_When_Provided()
    {
        AutoMock.Provide<IItem>(new MyItem());
        var optional = AutoMock.Resolve<Optional>();
        optional.Item.Should().NotBeNull();
        Action a = () => Moq.Mock.Get(optional);
        a.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoMock.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        a.Should().Throw<TestBootstrapException>();
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
