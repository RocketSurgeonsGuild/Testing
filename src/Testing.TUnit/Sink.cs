using System.Globalization;
using Serilog.Core;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

internal class Sink(TestContext context) : ILogEventSink
{
    public void Emit(LogEvent logEvent) => logEvent.RenderMessage(context.OutputWriter, CultureInfo.InvariantCulture);
}
