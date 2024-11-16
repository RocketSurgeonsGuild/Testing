using FakeItEasy.Creation;
using Serilog;

namespace Rocket.Surgery.Extensions.Testing;

public class AutoFakeTestContext
    (Action<AutoFakeTestContext, LoggerConfiguration>? configureLogger = null, Action<IFakeOptions>? fakeOptionsAction = null)
    : RocketSurgeryTestContext<AutoFakeTestContext>(configureLogger), IAutoFakeTestContext
{
    public Action<IFakeOptions>? FakeOptionsAction => fakeOptionsAction;
}
