using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using System;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoMockTestTests : AutoMockTest
    {
        public AutoMockTestTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        class Impl : AutoMockTest
        {
            public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
                Logger.LogError("abcd");
                Logger.LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Create_Usable_Logger()
        {
            var test = AutoMock.Resolve<Impl>();
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
            Action a = () => Mock.Get(optional);
            a.Should().Throw<ArgumentException>();
        }

        class MyItem : IItem { }

        public interface IItem { }

        class Optional
        {
            public IItem? Item { get; }

            public Optional(IItem? item = null) => Item = item;
        }
    }
}