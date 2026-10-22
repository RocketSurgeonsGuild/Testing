using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A test context that owns the Serilog logger for a test and streams its log events.
/// </summary>
public interface ILoggingTestContext : IDisposable, IObservable<LogEvent>
{
    /// <summary>
    ///     The logger for the test.
    /// </summary>
    ILogger Logger { get; }

    /// <summary>
    ///     Only include log events from the given source context.
    /// </summary>
    /// <param name="sourceContext">The source context to include.</param>
    void IncludeSourceContext(string sourceContext);

    /// <summary>
    ///     Exclude log events from the given source context.
    /// </summary>
    /// <param name="sourceContext">The source context to exclude.</param>
    void ExcludeSourceContext(string sourceContext);
}
