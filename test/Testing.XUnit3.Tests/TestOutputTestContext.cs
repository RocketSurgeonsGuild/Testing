using System.Globalization;
using FakeItEasy;
using FakeItEasy.Creation;
using Moq;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Display;
using Xunit.Sdk;
using Xunit.v3;

namespace Rocket.Surgery.Extensions.Testing.XUnit3.Tests;

public class TestOutputTestContext : RocketSurgeryTestContext<TestOutputTestContext>, IAutoFakeTestContext, IAutoMockTestContext, IAutoSubstituteTestContext
{
    private readonly LogEventLevel _logEventLevel;

    public TestOutputTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    {
        _logEventLevel = logEventLevel;
        TestOutputHelper = A.Fake<ITestOutputHelper>(z => z.Wrapping(testOutputHelper));
    }

    public ITestOutputHelper TestOutputHelper { get; }

    protected override void ConfigureLogger(TestOutputTestContext context, LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .MinimumLevel.Is(_logEventLevel)
           .WriteTo.Sink(new TestOutputSink(TestOutputHelper, new MessageTemplateTextFormatter(RocketSurgeonsTestingDefaults.LogFormat, CultureInfo.InvariantCulture)));
    }

    public Action<IFakeOptions>? FakeOptionsAction => null;
    public MockBehavior MockBehavior => MockBehavior.Default;
}

class TestOutputSink(ITestOutputHelper testOutputHelper, ITextFormatter textFormatter) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        ArgumentNullException.ThrowIfNull(logEvent);
        StringWriter output = new StringWriter();
        textFormatter.Format(logEvent, output);
        string message = output.ToString().Trim();
        testOutputHelper.WriteLine(message);
    }
}
