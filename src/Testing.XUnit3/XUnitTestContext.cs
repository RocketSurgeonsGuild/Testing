using Serilog;
using Serilog.Events;
using Xunit;
using Xunit.Sdk;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public abstract class XUnitTestContext<TContext> : RocketSurgeryTestContext<TContext>, ITestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext, ITestContext
{
    private readonly LogEventLevel _logEventLevel;
    private readonly ITestContext _testContext;

    /// <summary>
    ///     The xunit test context
    /// </summary>
    /// <param name="testContextAccessor"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    /// <param name="configureLogger"></param>
    protected XUnitTestContext(
        ITestContextAccessor testContextAccessor,
        LogEventLevel logEventLevel = LogEventLevel.Verbose,
        string? outputTemplate = null,
        Action<TContext, LoggerConfiguration>? configureLogger = null
    ) : base(configureLogger, logEventLevel, outputTemplate)
    {
        _testContext = testContextAccessor.Current;
        _logEventLevel = logEventLevel;
    }

    /// <inheritdoc />
    protected override void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new XUnitSink(context));
    }

    /// <inheritdoc />
    public void AddAttachment(string name, string value)
    {
        _testContext.AddAttachment(name, value);
    }

    /// <inheritdoc />
    public void AddAttachment(string name, byte[] value, string mediaType = "application/octet-stream")
    {
        _testContext.AddAttachment(name, value, mediaType);
    }

    /// <inheritdoc />
    public void AddWarning(string message)
    {
        _testContext.AddWarning(message);
    }

    /// <inheritdoc />
    public void CancelCurrentTest()
    {
        _testContext.CancelCurrentTest();
    }

    /// <inheritdoc />
    public void SendDiagnosticMessage(string message)
    {
        _testContext.SendDiagnosticMessage(message);
    }

    /// <inheritdoc />
    public void SendDiagnosticMessage(string format, object? arg0)
    {
        _testContext.SendDiagnosticMessage(format, arg0);
    }

    /// <inheritdoc />
    public void SendDiagnosticMessage(string format, object? arg0, object? arg1)
    {
        _testContext.SendDiagnosticMessage(format, arg0, arg1);
    }

    /// <inheritdoc />
    public void SendDiagnosticMessage(string format, object? arg0, object? arg1, object? arg2)
    {
        _testContext.SendDiagnosticMessage(format, arg0, arg1, arg2);
    }

    /// <inheritdoc />
    public void SendDiagnosticMessage(string format, params object?[] args)
    {
        _testContext.SendDiagnosticMessage(format, args);
    }

    /// <inheritdoc />
    public IReadOnlyDictionary<string, TestAttachment>? Attachments => _testContext.Attachments;

    /// <inheritdoc />
    public CancellationToken CancellationToken => _testContext.CancellationToken;

    /// <inheritdoc />
    public Dictionary<string, object?> KeyValueStorage => _testContext.KeyValueStorage;

    /// <inheritdoc />
    public TestPipelineStage PipelineStage => _testContext.PipelineStage;

    /// <inheritdoc />
    public ITest? Test => _testContext.Test;

    /// <inheritdoc />
    public ITestAssembly? TestAssembly => _testContext.TestAssembly;

    /// <inheritdoc />
    public TestEngineStatus? TestAssemblyStatus => _testContext.TestAssemblyStatus;

    /// <inheritdoc />
    public ITestCase? TestCase => _testContext.TestCase;

    /// <inheritdoc />
    public TestEngineStatus? TestCaseStatus => _testContext.TestCaseStatus;

    /// <inheritdoc />
    public ITestClass? TestClass => _testContext.TestClass;

    /// <inheritdoc />
    public object? TestClassInstance => _testContext.TestClassInstance;

    /// <inheritdoc />
    public TestEngineStatus? TestClassStatus => _testContext.TestClassStatus;

    /// <inheritdoc />
    public ITestCollection? TestCollection => _testContext.TestCollection;

    /// <inheritdoc />
    public TestEngineStatus? TestCollectionStatus => _testContext.TestCollectionStatus;

    /// <inheritdoc />
    public ITestMethod? TestMethod => _testContext.TestMethod;

    /// <inheritdoc />
    public TestEngineStatus? TestMethodStatus => _testContext.TestMethodStatus;

    /// <inheritdoc />
    public ITestOutputHelper? TestOutputHelper => _testContext.TestOutputHelper;

    /// <inheritdoc />
    public TestResultState? TestState => _testContext.TestState;

    /// <inheritdoc />
    public TestEngineStatus? TestStatus => _testContext.TestStatus;

    /// <inheritdoc />
    public IReadOnlyList<string>? Warnings => _testContext.Warnings;
}

/// <summary>
///     The xunit test context
/// </summary>
[PublicAPI]
public class XUnitTestContext(ITestContextAccessor testContextAccessor, LogEventLevel logEventLevel = LogEventLevel.Verbose, string? outputTemplate = null, Action<XUnitTestContext, LoggerConfiguration>? configureLogger = null)
    : XUnitTestContext<XUnitTestContext>(testContextAccessor, logEventLevel, outputTemplate, configureLogger);
