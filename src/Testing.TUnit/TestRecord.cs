using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
/// <remarks>
///     The xunit test context
/// </remarks>
/// <param name="logEventLevel"></param>
/// <param name="outputTemplate"></param>
/// <param name="configureLogger"></param>
[PublicAPI]
public abstract partial class TestRecord<TContext>
    (LogEventLevel logEventLevel = LogEventLevel.Verbose, string? outputTemplate = null, Action<TContext, LoggerConfiguration>? configureLogger = null)
    : RocketSurgeryTestContext<TContext>(configureLogger, logEventLevel, outputTemplate)
    where TContext : RocketSurgeryTestContext<TContext>
{
    /// <summary>
    /// Represents the current test context for xUnit tests.
    /// </summary>
    /// <remarks>
    /// Provides access to the active test context during the execution of a test.
    /// This property is typically used to retrieve contextual information or perform
    /// test-specific logging operations.
    /// </remarks>
    public TestContext TestContext { get; } = TestContext.Current!;

    private readonly LogEventLevel _logEventLevel = logEventLevel;

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration) =>
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new Sink(TestContext));
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public class TestRecord
(
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<TestRecord, LoggerConfiguration>? configureLogger = null)
    : TestRecord<TestRecord>(logEventLevel, outputTemplate, configureLogger)
{
    /// <summary>
    ///     Create the test record
    /// </summary>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <returns></returns>
    public static TestRecord Create(
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null
    ) => new(logEventLevel, outputTemplate);
}
