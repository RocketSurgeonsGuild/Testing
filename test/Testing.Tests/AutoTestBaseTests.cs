using FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests
{
    public class AutoTestBaseTests : AutoTestBase
    {
        public AutoTestBaseTests(ITestOutputHelper outputHelper) : base(outputHelper){ }

        class Impl : AutoTestBase
        {
            public Impl(ITestOutputHelper outputHelper) : base(outputHelper)
            {
                DefaultLogger.LogError("abcd");
                DefaultLogger.LogError("abcd {something}", "somevalue");
            }
        }

        [Fact]
        public void Should_Create_Usable_Logger()
        {
            var test = AutoFake.Resolve<Impl>();
            A.CallTo(() => AutoFake.Resolve<ITestOutputHelper>().WriteLine(A<string>._)).MustHaveHappened();
        }

        class LoggerImpl : AutoTestBase
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

        class LoggerFactoryImpl : AutoTestBase
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

        class GenericLoggerImpl : AutoTestBase
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


    }
}