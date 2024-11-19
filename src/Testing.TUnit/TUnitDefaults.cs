using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Defaults for logging with xunit
/// </summary>
[PublicAPI]
public static class TUnitDefaults
{
    /// <summary>
    ///     Create the test context
    /// </summary>
    /// <param name="testContext"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public static TUnitTestRecord CreateTestContext(
        TestContext testContext,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null
    ) =>
        new(testContext, logEventLevel, outputTemplate);
}
