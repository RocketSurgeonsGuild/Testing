using Moq;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Rocket.Surgery.Extensions.Testing;
using System;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using Autofac.Extras.FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac;

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
            var test = AutoMock.Create<Impl>();
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

        class MyItem { }
    }
}
