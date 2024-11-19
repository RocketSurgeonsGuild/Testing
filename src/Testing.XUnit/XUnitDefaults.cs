using System.Globalization;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
/// Defaults for logging with xunit
/// </summary>
[PublicAPI]
public static class XUnitDefaults
{
    /// <summary>
    /// Create the test context
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public static XUnitTestContext CreateTestContext(
        ITestOutputHelper outputHelper,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null
    )
    {
        return new(outputHelper, logEventLevel, outputTemplate);
    }
}
