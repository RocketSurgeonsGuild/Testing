using System.Reactive.Subjects;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
/// Generic text context used for logging and can be inherited to provide additional functionality
/// </summary>
/// <typeparam name="TContext"></typeparam>
public abstract class RocketSurgeryTestContext<TContext> : ILoggingTestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext
{
    private readonly Action<TContext, LoggerConfiguration>? _configureLogger;

    /// <summary>
    /// Create the base test context
    /// </summary>
    /// <param name="configureLogger"></param>
    /// <param name="logEventLevel"></param>
    /// <param name="outputTemplate"></param>
    protected RocketSurgeryTestContext(Action<TContext, LoggerConfiguration>? configureLogger = null, LogEventLevel logEventLevel = LogEventLevel.Verbose, string? outputTemplate = null)
    {
        _configureLogger = configureLogger;
        _loggerConfiguration
           .WriteTo.Observers(x => x.Subscribe(_logs))
           .MinimumLevel.Is(logEventLevel)
//                    .WriteTo.Spectre(RocketSurgeonsTestingDefaults.LogFormat)
           .Enrich.FromLogContext();
        FilterLogs(_loggerConfiguration);
        OutputTemplate = outputTemplate ?? RocketSurgeonsTestingDefaults.OutputTemplate;
    }

    #pragma warning disable CA1033
    ILogger ILoggingTestContext.Logger => _logger ??= CreateLogger();
    #pragma warning restore CA1033

    /// <summary>
    /// A method that can be overriden to configure the logger
    /// </summary>
    /// <param name="context"></param>
    /// <param name="loggerConfiguration"></param>
    protected virtual void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration) { }

    /// <summary>
    /// The output template for the logger
    /// </summary>
    protected virtual string OutputTemplate { get; }

    private Logger CreateLogger()
    {
        RocketSurgeonsTestingDefaults.ConfigureLogging?.Invoke(_loggerConfiguration);
        _configureLogger?.Invoke((TContext)this, _loggerConfiguration);
        ConfigureLogger((TContext)this, _loggerConfiguration);
        return _loggerConfiguration.CreateLogger();
    }

    private void FilterLogs(LoggerConfiguration loggerConfiguration)
    {
        loggerConfiguration
           .Filter.ByExcluding(
                x =>
                {
                    if (!x.Properties.TryGetValue("SourceContext", out var c))
                        return false;
                    if (c is not ScalarValue { Value: string sourceContext, })
                        return true;
                    return _excludeSourceContexts.Any(z => z.Equals(sourceContext, StringComparison.Ordinal));
                }
            )
           .Filter.ByIncludingOnly(
                x =>
                {
                    if (!x.Properties.TryGetValue("SourceContext", out var c))
                        return true;
                    if (c is not ScalarValue { Value: string sourceContext, })
                        return false;
                    return _includeSourceContexts.All(z => z.Equals(sourceContext, StringComparison.Ordinal));
                }
            );
    }

    private readonly List<string> _excludeSourceContexts = new();
    private readonly List<string> _includeSourceContexts = new();
    private readonly Subject<LogEvent> _logs = new();
    private readonly LoggerConfiguration _loggerConfiguration = new();
    private Logger? _logger;

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="sourceContext"></param>
    public void ExcludeSourceContext(string sourceContext)
    {
        if (string.IsNullOrWhiteSpace(sourceContext)) return;
        _excludeSourceContexts.Add(sourceContext);
    }

    /// <summary>
    ///     Filter a given source context from serilog
    /// </summary>
    /// <param name="sourceContext"></param>
    public void IncludeSourceContext(string sourceContext)
    {
        if (string.IsNullOrWhiteSpace(sourceContext)) return;
        _includeSourceContexts.Add(sourceContext);
    }

    /// <summary>
    /// The dispose method
    /// </summary>
    public void Dispose()
    {
        _logger?.Dispose();
        _logs.Dispose();
    }

    IDisposable IObservable<LogEvent>.Subscribe(IObserver<LogEvent> observer) => _logs.Subscribe(observer);
}

/// <summary>
/// The non generic test context
/// </summary>
/// <param name="configureLogger"></param>
public class RocketSurgeryTestContext
    (Action<RocketSurgeryTestContext, LoggerConfiguration> configureLogger) : RocketSurgeryTestContext<RocketSurgeryTestContext>(configureLogger);
