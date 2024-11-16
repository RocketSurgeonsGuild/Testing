using System.Reactive.Subjects;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

public abstract class RocketSurgeryTestContext<TContext> : ILoggingTestContext
    where TContext : RocketSurgeryTestContext<TContext>, ILoggingTestContext
{
    private readonly Action<TContext, LoggerConfiguration>? _configureLogger;

    protected RocketSurgeryTestContext(Action<TContext, LoggerConfiguration>? configureLogger = null)
    {
        _configureLogger = configureLogger;
        _loggerConfiguration
           .WriteTo.Observers(x => x.Subscribe(_logs))
//                    .WriteTo.Spectre(RocketSurgeonsTestingDefaults.LogFormat)
           .Enrich.FromLogContext();
        FilterLogs(_loggerConfiguration);
    }

    #pragma warning disable CA1033
    ILogger ILoggingTestContext.Logger => _logger ??= CreateLogger();
    #pragma warning restore CA1033

    protected virtual void ConfigureLogger(TContext context, LoggerConfiguration loggerConfiguration) { }

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

    public void Dispose()
    {
        _logger?.Dispose();
        _logs.Dispose();
    }

    IDisposable IObservable<LogEvent>.Subscribe(IObserver<LogEvent> observer) => _logs.Subscribe(observer);
}

public class RocketSurgeryTestContext
    (Action<RocketSurgeryTestContext, LoggerConfiguration> configureLogger) : RocketSurgeryTestContext<RocketSurgeryTestContext>(configureLogger);
