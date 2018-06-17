using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing
{
    public class TestDiagnosticLogger : ILogger
    {
        private readonly DiagnosticSource _diagnosticSource;
        internal static readonly IReadOnlyDictionary<LogLevel, string> Names =
            Enum.GetValues(typeof(LogLevel))
                .Cast<LogLevel>()
                .ToDictionary(x => x, x => $"Log.{x}");

        private static string GetName(LogLevel logLevel)
        {
            return Names.TryGetValue(logLevel, out var value) ? value : "Log.Other";
        }

        public TestDiagnosticLogger(DiagnosticSource diagnosticSource)
        {
            _diagnosticSource = diagnosticSource;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            _diagnosticSource.Write(GetName(logLevel), new
            {
                logLevel,
                eventId,
                state = (object)state,
                exception,
                message = formatter(state, exception)
            });
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return _diagnosticSource.IsEnabled(GetName(logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            var activity = _diagnosticSource.StartActivity(new Activity("Scope"), state);
            return new Disposable(_diagnosticSource, activity, state);
        }

        class Disposable : IDisposable
        {
            private readonly DiagnosticSource _diagnosticSource;
            private readonly Activity _activity;
            private readonly object _state;

            public Disposable(DiagnosticSource diagnosticSource, Activity activity, object state)
            {
                _diagnosticSource = diagnosticSource;
                _activity = activity;
                _state = state;
            }

            public void Dispose()
            {
                _diagnosticSource.StopActivity(_activity, _state);
            }
        }
    }
}
