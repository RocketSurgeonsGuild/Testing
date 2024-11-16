using System.Globalization;
using FakeItEasy;
using FakeItEasy.Creation;
using Moq;
using Serilog;
using Serilog.Events;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

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
           .WriteTo.TestOutput(TestOutputHelper, outputTemplate: RocketSurgeonsTestingDefaults.LogFormat, formatProvider: CultureInfo.InvariantCulture);
    }

    public Action<IFakeOptions>? FakeOptionsAction => null;
    public MockBehavior MockBehavior => MockBehavior.Default;
}