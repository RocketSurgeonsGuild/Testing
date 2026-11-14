using System.Globalization;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests;

public class TestOutputTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose) : RocketSurgeryTestContext<TestOutputTestContext>
{
    private readonly LogEventLevel _logEventLevel = logEventLevel;

    /// <summary>
    ///     A mock that records every call and forwards it to the real output helper, so tests can verify what was written.
    /// </summary>
    public ITestOutputHelperMock TestOutputHelper { get; } = CreateForwardingMock(testOutputHelper);

    protected override void ConfigureLogger(TestOutputTestContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new TestOutputSink(TestOutputHelper, new MessageTemplateTextFormatter(OutputTemplate, CultureInfo.InvariantCulture)));
    }

    // TUnit.Mocks cannot wrap an interface instance, so forward each member to the real helper explicitly.
    private static ITestOutputHelperMock CreateForwardingMock(ITestOutputHelper inner)
    {
        var mock = ITestOutputHelper.Mock();
        mock.Output.Returns(() => inner.Output);
        mock.Write(Any<string>()).Callback(inner.Write);
        mock.Write(Any<string>(), Any<object[]>()).Callback(inner.Write);
        mock.WriteLine(Any<string>()).Callback(inner.WriteLine);
        mock.WriteLine(Any<string>(), Any<object[]>()).Callback(inner.WriteLine);
        return mock;
    }
}

internal class TestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        var output = new StringWriter();
        textFormatter.Format(logEvent, output);
        var message = output.ToString().Trim();
        testOutputHelper.WriteLine(message);
    }
}
