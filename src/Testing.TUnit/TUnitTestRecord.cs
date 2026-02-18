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
        .WriteTo.Sink(new TUnitSink(_context));
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public class TUnitTestRecord
(
    TestContext context,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<TUnitTestRecord, LoggerConfiguration>? configureLogger = null)
    : TUnitTestRecord<TUnitTestRecord>(context, logEventLevel, outputTemplate, configureLogger);
