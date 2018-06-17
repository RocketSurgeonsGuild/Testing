using System;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    public class TestDiagnosticListenerLoggingAdapter
    {
        private readonly ILogger _logger;

        public TestDiagnosticListenerLoggingAdapter(ILogger logger)
        {
            _logger = logger;
        }

        [DiagnosticName("Log.Other")]
        public void LogOther(LogLevel logLevel, EventId eventId, Exception exception, string message) => _logger.Log(logLevel, eventId, exception, message);

        [DiagnosticName("Log.Trace")]
        public void LogTrace(EventId eventId, Exception exception, string message) => _logger.LogTrace(eventId, exception, message);

        [DiagnosticName("Log.Debug")]
        public void LogDebug(EventId eventId, Exception exception, string message) => _logger.LogDebug(eventId, exception, message);

        [DiagnosticName("Log.Information")]
        public void LogInformation(EventId eventId, Exception exception, string message) => _logger.LogInformation(eventId, exception, message);

        [DiagnosticName("Log.Warning")]
        public void LogWarning(EventId eventId, Exception exception, string message) => _logger.LogWarning(eventId, exception, message);

        [DiagnosticName("Log.Error")]
        public void LogError(EventId eventId, Exception exception, string message) => _logger.LogError(eventId, exception, message);

        [DiagnosticName("Log.Critical")]
        public void LogCritical(EventId eventId, Exception exception, string message) => _logger.LogCritical(eventId, exception, message);
    }
}