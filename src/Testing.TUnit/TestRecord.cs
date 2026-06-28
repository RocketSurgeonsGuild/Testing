using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
/// <remarks>
///     The xunit test context
/// </remarks>
/// <param name="context"></param>
/// <param name="logEventLevel"></param>
/// <param name="outputTemplate"></param>
/// <param name="configureLogger"></param>
[PublicAPI]
public abstract class TUnitTestRecord<TContext>(
    TestContext context,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<TContext, LoggerConfiguration>? configureLogger = null
    ) : RocketSurgeryTestContext<TContext>(
    configureLogger,
    logEventLevel,
    outputTemplate
    )
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext
{
    private readonly TestContext _context = context;
    private readonly LogEventLevel _logEventLevel = logEventLevel;

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration) => loggerConfiguration
        .MinimumLevel.Is(_logEventLevel)
        .WriteTo.Sink(new Sink(_context));
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public class TestRecord
(
    TestContext context,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<TestRecord, LoggerConfiguration>? configureLogger = null)
    : TUnitTestRecord<TestRecord>(context, logEventLevel, outputTemplate, configureLogger)
{
    /// <summary>
    ///     Create the test record
    /// </summary>
    /// <param name="testContext"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public static TestRecord Create(
        TestContext testContext,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null
    ) => new(testContext, logEventLevel, outputTemplate);
}
