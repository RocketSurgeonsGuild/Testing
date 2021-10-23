using JetBrains.Annotations;
using Microsoft.Extensions.DiagnosticAdapter;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     The test diagnostic listener allows you to wire in all log sources into a given Diagnostic Source
/// </summary>
[PublicAPI]
public class TestDiagnosticListenerLoggingAdapter
{
    private readonly ILogger _logger;

    /// <summary>
    ///     The default constructor
    /// </summary>
    /// <param name="logger"></param>
    public TestDiagnosticListenerLoggingAdapter(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="logLevel"></param>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Other")]
    public void LogOther(LogLevel logLevel, EventId eventId, Exception exception, string message) => LoggerMessage.Define(logLevel, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Trace")]
    public void LogTrace(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Trace, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Debug")]
    public void LogDebug(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Debug, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Information")]
    public void LogInformation(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Information, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Warning")]
    public void LogWarning(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Warning, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Error")]
    public void LogError(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Error, eventId, message)(_logger, exception);

    /// <summary>
    ///     A logging method
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="exception"></param>
    /// <param name="message"></param>
    [DiagnosticName("Log.Critical")]
    public void LogCritical(EventId eventId, Exception exception, string message) => LoggerMessage.Define(LogLevel.Critical, eventId, message)(_logger, exception);
}