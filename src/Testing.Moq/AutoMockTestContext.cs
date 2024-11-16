using Moq;
using Serilog;

namespace Rocket.Surgery.Extensions.Testing;

public class AutoMockTestContext(Action<AutoMockTestContext, LoggerConfiguration> configureLogger, MockBehavior mockBehavior = MockBehavior.Default)
    : RocketSurgeryTestContext<AutoMockTestContext>(configureLogger), IAutoMockTestContext
{
    public MockBehavior MockBehavior => mockBehavior;
}
