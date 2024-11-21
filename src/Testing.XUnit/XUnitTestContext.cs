using System.Globalization;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
public interface IXUnitTestContext
{
    /// <summary>
    ///     The current running test
    /// </summary>
    ITest Test { get; }

    /// <summary>
    ///     The XUnit Test Output Helper
    /// </summary>
    ITestOutputHelper TestOutputHelper { get; }
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public abstract class XUnitTestContext<TContext> : RocketSurgeryTestContext<TContext>, IXUnitTestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext, IXUnitTestContext
{
    private readonly LogEventLevel _logEventLevel;

    /// <summary>
    ///     The xunit test context
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <param name="configureLogger"></param>
    protected XUnitTestContext(
        ITestOutputHelper outputHelper,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null,
        Action<TContext, LoggerConfiguration>? configureLogger = null
    ) : base(configureLogger, logEventLevel, outputTemplate)
    {
        _logEventLevel = logEventLevel;
        TestOutputHelper = outputHelper;
        Test = outputHelper.GetTest()!;
    }

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.TestOutput(TestOutputHelper, outputTemplate: OutputTemplate, formatProvider: CultureInfo.InvariantCulture);
    }

    /// <summary>
    ///     The current running test
    /// </summary>
    public ITest Test { get; }

    /// <summary>
    ///     The XUnit Test Output Helper
    /// </summary>
    public ITestOutputHelper TestOutputHelper { get; }
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public class XUnitTestContext
(
    ITestOutputHelper outputHelper,
    LogEventLevel logEventLevel = LogEventLevel.Verbose,
    string? outputTemplate = null,
    Action<XUnitTestContext, LoggerConfiguration>? configureLogger = null)
    : XUnitTestContext<XUnitTestContext>(outputHelper, logEventLevel, outputTemplate, configureLogger)
{
    /// <summary>
    ///     Create the test context
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <param name="configureLogger"></param>
    /// <returns></returns>
    public static XUnitTestContext Create(
        ITestOutputHelper outputHelper,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null,
        Action<XUnitTestContext, LoggerConfiguration>? configureLogger = null
    ) => new(outputHelper, logEventLevel, outputTemplate, configureLogger);
}
