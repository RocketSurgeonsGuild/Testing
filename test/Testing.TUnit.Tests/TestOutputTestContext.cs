using FakeItEasy.Creation;
using Moq;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests;

public class TestOutputTestContext : RocketSurgeryTestContext<TestOutputTestContext>, IAutoFakeTestContext, IAutoMockTestContext, IAutoSubstituteTestContext
{
    private readonly LogEventLevel _logEventLevel;

    public TestOutputTestContext(LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        _logEventLevel = logEventLevel;
    }

    protected override void ConfigureLogger(TestOutputTestContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Spectre(outputTemplate: OutputTemplate);
    }

    public Action<IFakeOptions>? FakeOptionsAction => null;
    public MockBehavior MockBehavior => MockBehavior.Default;
}
