using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A simple base test class with logger, logger factory and diagnostic source all wired into the <see cref="ITestOutputHelper" />.
    /// </summary>
    public abstract class LoggerTest : IDisposable
    {
        /// <summary>
        /// The <see cref="ILoggerFactory" />
        /// </summary>
        protected readonly ILoggerFactory LoggerFactory;

        /// <summary>
        /// The <see cref="ILogger" />
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// The <see cref="XunitLoggerProvider" />
        /// </summary>
        protected readonly XunitLoggerProvider Provider;

        /// <summary>
        /// The <see cref="DiagnosticSource" />
        /// </summary>
        protected readonly DiagnosticSource DiagnosticSource;


        /// <summary>
        /// The <see cref="CompositeDisposable" />
        /// </summary>
        protected readonly CompositeDisposable Disposable;

        /// <summary>
        /// The default constructor with available logging level
        /// </summary>
        /// <param name="outputHelper"></param>
        /// <param name="minLevel"></param>
        /// <param name="loggerFactory"></param>
        /// <returns></returns>
        protected LoggerTest(ITestOutputHelper outputHelper, LogLevel minLevel = LogLevel.Information, ILoggerFactory loggerFactory = null)
        {
            Disposable = new CompositeDisposable();
            LoggerFactory = loggerFactory ?? new LoggerFactory();
            Provider = new XunitLoggerProvider(outputHelper, minLevel);
            LoggerFactory.AddProvider(Provider);
            Logger = LoggerFactory.CreateLogger("Default");

            var diagnosticListener = new DiagnosticListener("Test");
            Disposable.Add(diagnosticListener.SubscribeWithAdapter(new TestDiagnosticListenerLoggingAdapter(Logger)));
            DiagnosticSource = diagnosticListener;
        }

        void IDisposable.Dispose()
        {
            Disposable.Dispose();
        }
    }
}

