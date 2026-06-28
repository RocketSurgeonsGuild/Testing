using FakeItEasy.Creation;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Spectre;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests;

public class TestOutputTestContext(LogEventLevel logEventLevel = LogEventLevel.Verbose) : RocketSurgeryTestContext<TestOutputTestContext>
{
    private readonly LogEventLevel _logEventLevel = logEventLevel;

    protected override void ConfigureLogger(TestOutputTestContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Spectre(outputTemplate: OutputTemplate);
    }

    public Action<IFakeOptions>? FakeOptionsAction => null;
}
