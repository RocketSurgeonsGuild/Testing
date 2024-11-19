using System.Globalization;
using Serilog.Core;
using Serilog.Events;
using Xunit;

namespace Rocket.Surgery.Extensions.Testing;

class XUnitSink(ITestContext context) : ILogEventSink
{
    public void Emit(LogEvent logEvent)
    {
        context.TestOutputHelper?.WriteLine(logEvent.RenderMessage(CultureInfo.InvariantCulture));
    }
}
