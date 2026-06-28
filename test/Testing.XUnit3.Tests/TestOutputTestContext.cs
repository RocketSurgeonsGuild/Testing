using System.Globalization;
using FakeItEasy;
using FakeItEasy.Creation;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests;

public class TestOutputTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose) : RocketSurgeryTestContext<TestOutputTestContext>
{
    private readonly LogEventLevel _logEventLevel = logEventLevel;

    public ITestOutputHelper TestOutputHelper { get; } = A.Fake<ITestOutputHelper>(z => z.Wrapping(testOutputHelper));

    protected override void ConfigureLogger(TestOutputTestContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new TestOutputSink(TestOutputHelper, new MessageTemplateTextFormatter(OutputTemplate, CultureInfo.InvariantCulture)));
    }

    public Action<IFakeOptions>? FakeOptionsAction => null;
}

internal class TestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        var output = new StringWriter();
        textFormatter.Format(logEvent, output);
        string message = output.ToString().Trim();
        testOutputHelper.WriteLine(message);
    }
}
