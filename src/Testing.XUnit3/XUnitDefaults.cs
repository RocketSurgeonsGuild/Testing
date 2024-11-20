using Serilog.Events;
using Xunit;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Defaults for logging with xunit
/// </summary>
[PublicAPI]
public static class XUnitDefaults
{
    /// <summary>
    ///     Create the test context
    /// </summary>
    /// <param name="testContextAccessor"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public static XUnitTestContext CreateTestContext(
        ITestContextAccessor testContextAccessor,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null
    ) =>
        new(testContextAccessor, logEventLevel, outputTemplate);
}
