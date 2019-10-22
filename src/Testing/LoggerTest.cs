using System;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Xunit.Abstractions;
using System.Reactive.Disposables;
using Serilog;
using Serilog.Events;
using IMsftLogger = Microsoft.Extensions.Logging.ILogger;
using ISeriLogger = Serilog.ILogger;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A simple base test class with logger, logger factory and diagnostic source all wired into the <see cref="ITestOutputHelper" />.
    /// </summary>
    public abstract class LoggerTest : IDisposable
    {
        private readonly Lazy<(IMsftLogger logger, ILoggerFactory loggerFactory, ISeriLogger serilogLogger, DiagnosticSource diagnosticSource)> _values;

        /// <summary>
        /// The <see cref="ILoggerFactory" />
        /// </summary>
        protected ILoggerFactory LoggerFactory => _values.Value.loggerFactory;

        /// <summary>
        /// The <see cref="Microsoft.Extensions.Logging.ILogger" />
        /// </summary>
        protected IMsftLogger Logger => _values.Value.logger;

        /// <summary>
        /// The <see cref="Microsoft.Extensions.Logging.ILogger" />
        /// </summary>
        protected ISeriLogger SerilogLogger => _values.Value.serilogLogger;

        /// <summary>
        /// The <see cref="DiagnosticSource" />
        /// </summary>
        protected DiagnosticSource DiagnosticSource => _values.Value.diagnosticSource;

        /// <summary>
        /// The <see cref="CompositeDisposable" />
        /// </summary>
        protected readonly CompositeDisposable Disposable;

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <returns></returns>
        protected LoggerTest(ITestOutputHelper outputHelper, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LogEventLevel.Information, logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <returns></returns>
        protected LoggerTest(ITestOutputHelper outputHelper, LogLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
            : this(outputHelper, LevelConvert.ToSerilogLevel(minLevel), logFormat, configureLogger)
        {
        }

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="logFormat"></param>
        /// <param name="configureLogger"></param>
        /// <returns></returns>
        protected LoggerTest(ITestOutputHelper outputHelper, LogEventLevel minLevel, string logFormat = "[{Timestamp:HH:mm:ss} {Level:w4}] {Message}{NewLine}{Exception}", Action<LoggerConfiguration>? configureLogger = null)
        {
            Disposable = new CompositeDisposable();

            _values = new Lazy<(IMsftLogger logger, ILoggerFactory loggerFactory, ISeriLogger serilogLogger, DiagnosticSource diagnosticSource)>(() =>
            {
                var config = new LoggerConfiguration()
                    .WriteTo.TestOutput(outputHelper, LogEventLevel.Verbose, logFormat)
                    .MinimumLevel.Is(minLevel);
                configureLogger?.Invoke(config);
                var logger = config.CreateLogger();

                var loggerProviderCollection = new LoggerProviderCollection();
                var factory = new SerilogLoggerFactory(logger, false, loggerProviderCollection);
                var container = new ServiceCollection().AddLogging().AddSingleton(factory).BuildServiceProvider();
                Disposable.Add(container);
                Disposable.Add(logger);

                var diagnosticListener = new DiagnosticListener("Test");
                var defaultLogger = factory.CreateLogger("Default");
                Disposable.Add(diagnosticListener.SubscribeWithAdapter(new TestDiagnosticListenerLoggingAdapter(defaultLogger)));

                return (defaultLogger, factory, logger, diagnosticListener);
            });
        }

        void IDisposable.Dispose()
        {
            Disposable.Dispose();
        }
    }
}

