using System.Globalization;
using Serilog.Core;
using Serilog.Events;

namespace Rocket.Surgery.Extensions.Testing;

internal class TUnitSink(TestContext context) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        logEvent.RenderMessage(context.OutputWriter, CultureInfo.InvariantCulture);
    }
}
