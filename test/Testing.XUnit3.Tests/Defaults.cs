using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests;

internal static class Defaults
{
    public static TestOutputTestContext CreateTestOutput(ITestOutputHelper outputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        return new(outputHelper, logEventLevel);
    }
}
