using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests;

internal static class Defaults
{
    public static TestOutputTestContext CreateTestOutput(LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        return new(logEventLevel);
    }
}
