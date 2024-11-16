using Serilog;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

public interface ILoggingTestContext : IDisposable, IObservable<LogEvent>
{
    ILogger Logger { get; }
    void IncludeSourceContext(string sourceContext);
    void ExcludeSourceContext(string sourceContext);
}