using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public abstract class TUnitTestRecord<TContext> : RocketSurgeryTestContext<TContext>, ITUnitTestRecord
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext, ITUnitTestRecord
{
    private readonly TestContext _context;
    private readonly LogEventLevel _logEventLevel;

    /// <summary>
    ///     The xunit test context
    /// </summary>
    /// <param name="context"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <param name="configureLogger"></param>
    protected TUnitTestRecord(
        TestContext context,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null,
        Action<TContext, LoggerConfiguration>? configureLogger = null
    ) : base(
        configureLogger,
        logEventLevel,
        outputTemplate
    )
    {
        _context = context;
        _logEventLevel = logEventLevel;
    }

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new TUnitSink(_context));
    }

    /// <inheritdoc />
    public TestContextEvents Events => _context.Events;

    /// <inheritdoc />
    public DateTimeOffset? TestStart => _context.TestStart;

    /// <inheritdoc />
    public TestDetails TestDetails => _context.TestDetails;

    /// <inheritdoc />
    public int CurrentRetryAttempt => _context.CurrentRetryAttempt;

    /// <inheritdoc />
    public IReadOnlyList<ArgumentDisplayFormatter> ArgumentDisplayFormatters => _context.ArgumentDisplayFormatters;

    /// <inheritdoc />
    public IReadOnlyList<Timing> Timings => _context.Timings;

    /// <inheritdoc />
    public Dictionary<string, object?> ObjectBag => _context.ObjectBag;

    public TestResult? Result => _context.Result;

    /// <inheritdoc />
    public void SuppressReportingResult()
    {
        _context.SuppressReportingResult();
    }

    /// <inheritdoc />
    public void AddArtifact(Artifact artifact)
    {
        _context.AddArtifact(artifact);
    }
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
