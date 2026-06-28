using Serilog;
using Serilog.Events;
using Xunit;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
/// <remarks>
///     The xunit test context
/// </remarks>
/// <param name="testContextAccessor"></param>
/// <param name="logEventLevel"></param>
/// <param name="outputTemplate"></param>
/// <param name="configureLogger"></param>
[PublicAPI]
public abstract partial class XUnitTestContext<TContext>(
    ITestContextAccessor testContextAccessor,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<TContext, LoggerConfiguration>? configureLogger = null
    ) : RocketSurgeryTestContext<TContext>(configureLogger, logEventLevel, outputTemplate), ITestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext, ITestContext
{
    private readonly LogEventLevel _logEventLevel = logEventLevel;
    [BeaKona.AutoInterface(TemplateFileName = "template.scriban")]
    private readonly ITestContext _testContext = testContextAccessor.Current;

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration) => loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new XUnitSink(context));

}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class XUnitTestContext
(
    ITestContextAccessor testContextAccessor,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<XUnitTestContext, LoggerConfiguration>? configureLogger = null)
    : XUnitTestContext<XUnitTestContext>(testContextAccessor, logEventLevel, outputTemplate, configureLogger)
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay => ToString() ?? "";
}
