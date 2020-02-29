using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// The test diagnostic listener allows you to wire in all log sources into a given Diagnostic Source
    /// </summary>
    public class TestDiagnosticListenerLoggingAdapter
    {
        private readonly ILogger _logger;

        /// <summary>
        /// The default constructor
        /// </summary>
        /// <param name="logger"></param>
        public TestDiagnosticListenerLoggingAdapter(ILogger logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="logLevel"></param>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Other")]
        public void LogOther(LogLevel logLevel, EventId eventId, Exception exception, string message) => _logger.Log(logLevel, eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Trace")]
        public void LogTrace(EventId eventId, Exception exception, string message) => _logger.LogTrace(eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Debug")]
        public void LogDebug(EventId eventId, Exception exception, string message) => _logger.LogDebug(eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Information")]
        public void LogInformation(EventId eventId, Exception exception, string message) => _logger.LogInformation(eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Warning")]
        public void LogWarning(EventId eventId, Exception exception, string message) => _logger.LogWarning(eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Error")]
        public void LogError(EventId eventId, Exception exception, string message) => _logger.LogError(eventId, exception, message);

        /// <summary>
        /// A logging method
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="exception"></param>
        /// <param name="message"></param>
        [DiagnosticName("Log.Critical")]
        public void LogCritical(EventId eventId, Exception exception, string message) => _logger.LogCritical(eventId, exception, message);
    }
}
