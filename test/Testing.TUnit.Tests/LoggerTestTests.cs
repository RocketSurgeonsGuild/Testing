using FluentAssertions;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing.TUnit.Tests;

public class LoggerTestTests() : LoggerTest<TestOutputTestContext>(Defaults.CreateTestOutput())
{
    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
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

    [Test]
    public Task Should_Exclude_SourceContext_Messages()
    {
        using var listener = CaptureLogs(out var logs);
        var Factory = CreateLoggerFactory();
        var logger = Factory.CreateLogger("MyLogger");
        logger.LogInformation("Info");

        logs.Should().HaveCount(1);

        ExcludeSourceContext("MyLogger");
        logger.LogInformation("Info");

        logs.Should().HaveCount(1);
        return Task.CompletedTask;
    }

    [Test]
    public Task Should_Include_SourceContext_Messages()
    {
        using var listener = CaptureLogs(out var logs);
        var Factory = CreateLoggerFactory();
        var logger = Factory.CreateLogger("MyLogger");
        var otherLogger = Factory.CreateLogger("OtherLogger");

        logger.LogInformation("Info");
        otherLogger.LogInformation("Info");

        logs.Should().HaveCount(2);

        IncludeSourceContext("MyLogger");
        logger.LogInformation("Info");
        otherLogger.LogInformation("Info");

        logs.Should().HaveCount(3);
        return Task.CompletedTask;
    }
}
