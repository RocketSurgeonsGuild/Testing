using FakeItEasy.Creation;
using Moq;
using Serilog.Events;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

public class TestOutputTestContext(ITestOutputHelper testOutputHelper, LogEventLevel logEventLevel = LogEventLevel.Verbose)
    : XUnitTestContext<TestOutputTestContext>(testOutputHelper, logEventLevel), IAutoFakeTestContext,
        IAutoMockTestContext, IAutoSubstituteTestContext
{
    public Action<IFakeOptions>? FakeOptionsAction => null;
    public MockBehavior MockBehavior => MockBehavior.Default;
}
