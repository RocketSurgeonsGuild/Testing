using System;
using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;
using Rocket.Surgery.Extensions.Testing;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoFakeTestTests : AutoFakeTest
    {
        public AutoFakeTestTests(ITestOutputHelper outputHelper) : base(outputHelper) { }

        class Impl : AutoFakeTest
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
            var test = AutoFake.Resolve<Impl>();
            A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
        }

        class LoggerImpl : AutoFakeTest
        {
            public LoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
            }

            public void Write()
            {
                AutoFake.Resolve<ILogger>().LogError("abcd");
                AutoFake.Resolve<ILogger>().LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Inject_Logger()
        {
            var test = AutoFake.Resolve<LoggerImpl>();
            test.Write();
            A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
        }

        class LoggerFactoryImpl : AutoFakeTest
        {
            public LoggerFactoryImpl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
            }

            public void Write()
            {
                AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd");
                AutoFake.Resolve<ILoggerFactory>().CreateLogger("").LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Inject_LoggerFactory()
        {
            var test = AutoFake.Resolve<LoggerFactoryImpl>();
            test.Write();
            A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
        }

        class GenericLoggerImpl : AutoFakeTest
        {
            public GenericLoggerImpl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
            }

            public void Write()
            {
                AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd");
                AutoFake.Resolve<ILogger<GenericLoggerImpl>>().LogError("abcd {something}", "somevalue");
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
            ServiceProvider.GetRequiredService<IServiceProvider>().Should().BeOfType<AutoFakeServiceProvider>();
        }

        class MyItem
        {
        }
    }
}
