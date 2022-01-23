using System.Diagnostics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using IMsftLogger = Microsoft.Extensions.Logging.ILogger;
using ISeriLogger = Serilog.ILogger;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A simple base test class with logger, logger factory and diagnostic source all wired into the <see cref="ITestOutputHelper" />.
/// </summary>
[PublicAPI]
#pragma warning disable CA1063
public abstract class LoggerTest : IDisposable
#pragma warning restore CA1063
{
    private readonly Lazy<(IMsftLogger logger, ILoggerFactory loggerFactory, ISeriLogger serilogLogger, DiagnosticSource diagnosticSource, IObservable<LogEvent>
        logStream)> _values;

    private readonly List<string> _excludeSourceContexts = new List<string>();
    private readonly List<string> _includeSourceContexts = new List<string>();

    /// <summary>
    ///     The <see cref="ILoggerFactory" />
    /// </summary>
    protected ILoggerFactory LoggerFactory => _values.Value.loggerFactory;

    /// <summary>
    ///     The <see cref="Microsoft.Extensions.Logging.ILogger" />
    /// </summary>
    protected IMsftLogger Logger => _values.Value.logger;

    /// <summary>
    ///     The <see cref="Microsoft.Extensions.Logging.ILogger" />
    /// </summary>
    protected ISeriLogger SerilogLogger => _values.Value.serilogLogger;

    /// <summary>
    ///     The <see cref="DiagnosticSource" />
    /// </summary>
    protected DiagnosticSource DiagnosticSource => _values.Value.diagnosticSource;

    protected ITestOutputHelper TestOutputHelper { get; }

    /// <summary>
    ///     The <see cref="IObservable{LogEvent}" />
    /// </summary>
    protected IObservable<LogEvent> LogStream => _values.Value.logStream;

#pragma warning disable CA1051 // Do not declare visible instance fields
#pragma warning disable IDE1006 // Naming Styles
    /// <summary>
    ///     The <see cref="CompositeDisposable" />
    /// </summary>
    protected readonly CompositeDisposable Disposables;
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore CA1051 // Do not declare visible instance fields

#pragma warning disable RS0026 // Do not add multiple public overloads with optional parameters
    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <returns></returns>
    protected LoggerTest(
        ITestOutputHelper outputHelper, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
        Action<LoggerConfiguration>? configureLogger = null
    )
        : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger)
    {
    }

    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <returns></returns>
    protected LoggerTest(
        ITestOutputHelper outputHelper, LogLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
        Action<LoggerConfiguration>? configureLogger = null
    )
        : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger)
    {
    }

    /// <summary>
    ///     The default constructor with available logging level
    /// </summary>
    /// <param name="outputHelper"></param>
    /// <param name="minLevel"></param>
    /// <param name="logFormat"></param>
    /// <param name="configureLogger"></param>
    /// <returns></returns>
    protected LoggerTest(
        ITestOutputHelper outputHelper, LogEventLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}",
        Action<LoggerConfiguration>? configureLogger = null
    )
    {
        Disposables = new CompositeDisposable();
        TestOutputHelper = outputHelper;

        _values =
            new Lazy<(IMsftLogger logger, ILoggerFactory loggerFactory, ISeriLogger serilogLogger, DiagnosticSource diagnosticSource, IObservable<LogEvent>
                logStream)>(
                () =>
                {
                    var subject = new Subject<LogEvent>();
                    var config = new LoggerConfiguration()
                                .WriteTo.TestOutput(outputHelper, LogEventLevel.Verbose, logFormat)
                                .WriteTo.Observers(x => x.Subscribe(subject))
                                .MinimumLevel.Is(minLevel)
                                .Enrich.FromLogContext();
                    FilterLogs(config);
                    configureLogger?.Invoke(config);
                    var logger = config.CreateLogger();

                    var loggerProviderCollection = new LoggerProviderCollection();
                    var factory = CreateLoggerFactory(logger, loggerProviderCollection);
                    var container = new ServiceCollection().AddLogging().AddSingleton(factory).BuildServiceProvider();
                    Disposables.Add(container);
                    Disposables.Add(logger);

                    var diagnosticListener = new DiagnosticListener("Test");
                    Disposables.Add(
                        diagnosticListener.SubscribeWithAdapter(new TestDiagnosticListenerLoggingAdapter(factory.CreateLogger("DiagnosticSource")))
                    );

                    return ( factory.CreateLogger("Default"), factory, logger, diagnosticListener, subject );
                }
            );
    }
#pragma warning restore RS0026 // Do not add multiple public overloads with optional parameters

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
    protected virtual ILoggerFactory CreateLoggerFactory(ISeriLogger logger, LoggerProviderCollection loggerProviderCollection)
    {
        return new SerilogLoggerFactory(logger, false, loggerProviderCollection);
    }

#pragma warning disable CA1063
#pragma warning disable CA1816
#pragma warning disable CA1033
    void IDisposable.Dispose()
#pragma warning restore CA1033
#pragma warning restore CA1816
#pragma warning restore CA1063
    {
        Disposables.Dispose();
    }

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="context"></param>
    protected void ExcludeSourceContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context)) return;
        _excludeSourceContexts.Add(context);
    }

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="context"></param>
    protected void IncludeSourceContext(string context)
    {
        if (string.IsNullOrWhiteSpace(context)) return;
        _includeSourceContexts.Add(context);
    }

    private void FilterLogs(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .Filter.ByExcluding(
                x =>
                {
                    if (!x.Properties.TryGetValue("SourceContext", out var c) || c is not ScalarValue { Value: string sourceContext })
                        return false;
                    return _excludeSourceContexts.Any(z => z.Equals(sourceContext, StringComparison.Ordinal));
                }
            )
           .Filter.ByIncludingOnly(
                x =>
                {
                    if (!x.Properties.TryGetValue("SourceContext", out var c) || c is not ScalarValue { Value: string sourceContext })
                        return false;
                    return _includeSourceContexts.All(z => z.Equals(sourceContext, StringComparison.Ordinal));
                }
            );
    }
}
