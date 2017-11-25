using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public abstract class TestBase
    {
        protected readonly LoggerFactory LoggerFactory;
        protected readonly ILogger Logger;

        protected TestBase(ITestOutputHelper outputHelper)
        {
            LoggerFactory = new LoggerFactory();
            LoggerFactory.AddProvider(new XunitLoggerProvider(outputHelper));
            Logger = LoggerFactory.CreateLogger("Default");
        }
    }
}
