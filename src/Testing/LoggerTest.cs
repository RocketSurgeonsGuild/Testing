using System.Reactive.Disposables;
using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using ILogger = Serilog.ILogger;

#pragma warning disable CA1033
#pragma warning disable CA1063 // Implement IDisposable Correctly
#pragma warning disable CA1816
namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A simple base test class with logger
/// </summary>
[PublicAPI]
public abstract class LoggerTest<TContext>(TContext testContext) : IDisposable where TContext : class, ILoggingTestContext
{
    /// <summary>
    ///     The <see cref="TestContext" />
    /// </summary>
    protected TContext TestContext => testContext;

    /// <summary>
    ///     The <see cref="CompositeDisposable" />
    /// </summary>
    protected CompositeDisposable Disposables { get; } = new();

    /// <summary>
    ///     The <see cref="Serilog.ILogger" />
    /// </summary>
    protected ILogger Logger => testContext.Logger;

    /// <summary>
    ///     The <see cref="IObservable{LogEvent}" />
    /// </summary>
    protected IObservable<LogEvent> LogStream => testContext;

    /// <summary>
    ///     Capture log outputs until the item is disposed
    /// </summary>
    /// <param name="logs">The output logs</param>
    /// <returns></returns>
    protected IDisposable CaptureLogs(out IEnumerable<LogEvent> logs)
    {
        var list = new List<LogEvent>();
        logs = list;
        return LogStream
           .Subscribe(x => list.Add(x));
    }

    /// <summary>
    ///     Capture log outputs until the item is disposed
    /// </summary>
    /// <param name="logs">The output logs</param>
    /// <param name="filterLogs">A delegate to filter the logs to only a subset of the expected logs</param>
    /// <returns></returns>
    protected IDisposable CaptureLogs(Func<LogEvent, bool> filterLogs, out IEnumerable<LogEvent> logs)
    {
        var list = new List<LogEvent>();
        logs = list;
        return LogStream
              .Where(filterLogs)
              .Subscribe(x => list.Add(x));
    }

    /// <summary>
    ///     Control the way that the serilog logger factory is created.
    /// </summary>
    protected virtual ILoggerFactory CreateLoggerFactory(LoggerProviderCollection? loggerProviderCollection = null) =>
        new SerilogLoggerFactory(Logger, false, loggerProviderCollection);

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="context"></param>
    protected void ExcludeSourceContext(string context) => testContext.ExcludeSourceContext(context);

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="context"></param>
    protected void IncludeSourceContext(string context) => testContext.IncludeSourceContext(context);

    void IDisposable.Dispose()
    {
        Disposables.Dispose();
        testContext.Dispose();
    }
}

public class LoggerTest : LoggerTest<RocketSurgeryTestContext>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoggerTest" /> class.
    /// </summary>
    public LoggerTest(Action<RocketSurgeryTestContext, LoggerConfiguration> configureLogger) : base(new(configureLogger)) { }
}
