using System.Globalization;
using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
/// The xunit test context
/// </summary>
public interface IXUnitTestContext
{
    /// <summary>
    /// The current running test
    /// </summary>
    ITest Test { get; }

    /// <summary>
    /// The XUnit Test Output Helper
    /// </summary>
    ITestOutputHelper TestOutputHelper { get; }
}

/// <summary>
/// The xunit test context
/// </summary>
[PublicAPI]
public abstract class XUnitTestContext<TContext> : RocketSurgeryTestContext<TContext>, IXUnitTestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext, IXUnitTestContext
{
    private readonly LogEventLevel _logEventLevel;

    /// <summary>
    /// The xunit test context
    /// </summary>
    /// <param name="testOutputHelper"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    protected XUnitTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose, string? outputTemplate = null) : base(outputTemplate: outputTemplate)
    {
        _logEventLevel = logEventLevel;
        TestOutputHelper = testOutputHelper;
        Test = testOutputHelper.GetTest();
    }

    /// <summary>
    /// The current running test
    /// </summary>
    public ITest Test { get; }

    /// <summary>
    /// The XUnit Test Output Helper
    /// </summary>
    public ITestOutputHelper TestOutputHelper { get; }

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.TestOutput(TestOutputHelper, outputTemplate: OutputTemplate, formatProvider: CultureInfo.InvariantCulture);
    }
}

/// <summary>
/// The xunit test context
/// </summary>
[PublicAPI]
public class XUnitTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose, string? outputTemplate = null)
    : XUnitTestContext<XUnitTestContext>(testOutputHelper, logEventLevel, outputTemplate);
