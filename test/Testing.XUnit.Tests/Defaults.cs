using Serilog.Events;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

internal static class Defaults
{
    public static TestOutputTestContext CreateTestOutput(ITestOutputHelper outputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        return new(outputHelper, logEventLevel);
    }
}