using DryIoc;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests.Mock;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class AutoMockTestTests(ITestOutputHelper outputHelper) : AutoMockTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
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
        _ = AutoMock.Resolve<Impl>();
        AutoMock.Mock<ITestOutputHelper>().Verify(x => x.WriteLine(It.IsAny<string>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Should_Provide_Values()
    {
        var item = AutoMock.Provide(new MyItem());
        ServiceProvider.GetRequiredService<MyItem>().ShouldBeSameAs(item);
    }

    [Fact]
    public void Should_Return_Self_For_ServiceProvider() => ServiceProvider.GetRequiredService<IServiceProvider>().ShouldBe(ServiceProvider);

    [Fact]
    public void Should_Not_Mock_Optional_Parameters() => AutoMock.Resolve<Optional>().Item.ShouldBeNull();

    [Fact]
    public void Should_Populate_Optional_Parameters_When_Provided()
    {
        _ = AutoMock.Provide<IItem>(new MyItem());
        var optional = AutoMock.Resolve<Optional>();
        _ = optional.Item.ShouldNotBeNull();
        Action a = () => Moq.Mock.Get(optional);
        _ = a.ShouldThrow<ArgumentException>();
    }

    [Fact]
    public void Should_Fail_If_Container_Is_Touched_When_Building()
    {
        var access = AutoMock.Resolve<DoubleAccess>();
        Action a = () => access.Self.Resolve<IContainer>();
        _ = a.ShouldThrow<TestBootstrapException>();
    }

    private class Impl : AutoMockTest<XUnitTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(XUnitDefaults.CreateTestContext(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }

    private class DoubleAccess(ITestOutputHelper outputHelper) : AutoMockTest<XUnitTestContext>(XUnitDefaults.CreateTestContext(outputHelper))
    {
        public IContainer Self => Container;

        protected override IContainer BuildContainer(IContainer container) =>
            // invalid do not touch ServiceProvider or Container while constructing the container....
            Container.GetRequiredService<IContainer>();
    }

    private class MyItem : IItem;

    internal interface IItem;

    private class Optional(IItem? item = null)
    {
        public IItem? Item { get; } = item;
    }
}
