using Autofac.Extras.FakeItEasy;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public abstract class AutoTestBase : TestBase
    {
        protected  readonly AutoFake AutoFake;

        protected AutoTestBase(ITestOutputHelper outputHelper) : base(outputHelper)
        {
            AutoFake = new AutoFake();
            AutoFake.Container.ComponentRegistry.AddRegistrationSource(
                new LoggingRegistrationSource(LoggerFactory)
            );
        }
    }
}