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
    public abstract class TestBase : IDisposable
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected readonly ILogger Logger;
        protected readonly XunitLoggerProvider Provider;
        protected readonly DiagnosticSource DiagnosticSource;
        protected CompositeDisposable Disposable;

        protected TestBase(ITestOutputHelper outputHelper) : this(outputHelper, LogLevel.Information)
        {
        }

        protected TestBase(ITestOutputHelper outputHelper, LogLevel minLevel)
        {
            Disposable = new CompositeDisposable();
            var diagnosticListener = new DiagnosticListener("Test");
            Disposable.Add(diagnosticListener.SubscribeWithAdapter(new TestDiagnosticListenerLoggingAdapter(Logger)));
            DiagnosticSource = diagnosticListener;

            LoggerFactory = new TestLoggerFactory();
            Provider = new XunitLoggerProvider(outputHelper, minLevel);
            LoggerFactory.AddProvider(Provider);
            Logger = LoggerFactory.CreateLogger("Default");
        }

        public void Dispose()
        {
            Disposable.Dispose();
        }
    }
}

