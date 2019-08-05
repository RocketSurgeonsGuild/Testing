using Autofac.Extras.FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public abstract class AutoTestBase : TestBase
    {
        protected  readonly AutoFake AutoFake;

        protected AutoTestBase(ITestOutputHelper outputHelper) : this(outputHelper, LogLevel.Information)
        {
        }

        protected AutoTestBase(ITestOutputHelper outputHelper, LogLevel minLevel) : base(outputHelper, minLevel)
        {
            AutoFake = new AutoFake();
            AutoFake.Container.ComponentRegistry.AddRegistrationSource(
                new LoggingRegistrationSource(LoggerFactory, Logger)
            );
        }
    }
    public abstract class AutoTestHostBase : AutoTestBase
    {
        protected  readonly AutoFake AutoFake;

        protected AutoTestBase(ITestOutputHelper outputHelper) : this(outputHelper, LogLevel.Information)
        {
        }

        protected AutoTestBase(ITestOutputHelper outputHelper, LogLevel minLevel) : base(outputHelper, minLevel)
        {
            AutoFake = new AutoFake();
            AutoFake.Container.ComponentRegistry.AddRegistrationSource(
                new LoggingRegistrationSource(LoggerFactory, Logger)
            );
        }
    }
}
