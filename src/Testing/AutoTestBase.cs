using Autofac.Extras.FakeItEasy;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A base class with AutoFake wired in for Autofac
    /// </summary>
    public abstract class AutoTestBase : TestBase
    {
        /// <summary>
        /// The AutoFake instance
        /// </summary>
        protected readonly AutoFake AutoFake;

        /// <summary>
        /// Create the auto test class
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        protected AutoTestBase(ITestOutputHelper outputHelper, LogLevel minLevel = LogLevel.Information) : base(outputHelper, minLevel)
        {
            AutoFake = new AutoFake();
            AutoFake.Container.ComponentRegistry.AddRegistrationSource(
                new LoggingRegistrationSource(LoggerFactory, Logger)
            );
        }
    }
}
