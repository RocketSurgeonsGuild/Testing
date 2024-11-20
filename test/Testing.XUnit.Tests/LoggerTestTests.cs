using FakeItEasy;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Serilog.Events;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.XUnit.Tests;

public class LoggerTestTests : LoggerTest<XUnitTestContext>
{
    [Fact]
    public Task Should_Create_Usable_Logger()
    {
        var helper = A.Fake<ITestOutputHelper>();
        using var _ = new Impl(helper);
        A.CallTo(() => helper.WriteLine(A<string>._)).MustHaveHappened();
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Create_A_Log_Stream()
    {
        Logger.Information("this is a test 1");

        using var listener = CaptureLogs(out var logs);
        using (listener)
        {
            Logger.Information("this is a test 2");
        }

        Logger.Information("this is a test 3");
        logs.Should().HaveCount(1);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Create_A_Log_Filter_Stream()
    {
        Logger.Information("this is a test 1");

        using var listener = CaptureLogs(log => log.Level >= LogEventLevel.Warning, out var logs);
        using (listener)
        {
            Logger.Information("this is a test 2");
            Logger.Warning("this is a test 2");
            Logger.Error("this is a test 2");
            Logger.Fatal("this is a test 2");
        }

        Logger.Information("this is a test 3");
        logs.Should().HaveCount(3);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Create_A_Serilog_Stream()
    {
        Logger.Information("this is a test 1");

        using var listener = CaptureLogs(out var logs);
        using (listener)
        {
            Logger.Information("this is a test 2");
        }

        Logger.Information("this is a test 3");
        logs.Should().HaveCount(1);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Create_A_Log_Serilog_Stream()
    {
        Logger.Information("this is a test 1");

        using var listener = CaptureLogs(log => log.Level >= LogEventLevel.Warning, out var logs);
        using (listener)
        {
            Logger.Information("this is a test 2");
            Logger.Warning("this is a test 2");
            Logger.Error("this is a test 2");
            Logger.Fatal("this is a test 2");
        }

        Logger.Information("this is a test 3");
        logs.Should().HaveCount(3);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Exclude_SourceContext_Messages()
    {
        using var listener = CaptureLogs(out var logs);
        var factory = CreateLoggerFactory();
        var logger = factory.CreateLogger("MyLogger");
        logger.LogInformation("Info");

        logs.Should().HaveCount(1);

        ExcludeSourceContext("MyLogger");
        logger.LogInformation("Info");

        logs.Should().HaveCount(1);
        return Task.CompletedTask;
    }

    [Fact]
    public Task Should_Include_SourceContext_Messages()
    {
        using var listener = CaptureLogs(out var logs);
        var factory = CreateLoggerFactory();
        var logger = factory.CreateLogger("MyLogger");
        var otherLogger = factory.CreateLogger("OtherLogger");

        logger.LogInformation("Info");
        otherLogger.LogInformation("Info");

        logs.Should().HaveCount(2);

        IncludeSourceContext("MyLogger");
        logger.LogInformation("Info");
        otherLogger.LogInformation("Info");

        logs.Should().HaveCount(3);
        return Task.CompletedTask;
    }

    public LoggerTestTests(ITestOutputHelper testOutputHelper) : base(XUnitDefaults.CreateTestContext(testOutputHelper)) =>
        // this is the wrapped one.
        _testOutputHelper = TestContext.TestOutputHelper;

    private readonly ITestOutputHelper _testOutputHelper;

    private class Impl : LoggerTest<TestOutputTestContext>
    {
        public Impl(ITestOutputHelper outputHelper) : base(Defaults.CreateTestOutput(outputHelper))
        {
            Logger.Error("abcd");
            Logger.Error("abcd {Something}", "somevalue");
        }
    }
}
