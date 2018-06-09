using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing
{
    public abstract class TestBase
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected readonly ILogger Logger;
        protected readonly XunitLoggerProvider Provider;

        protected TestBase(ITestOutputHelper outputHelper) : this(outputHelper, LogLevel.Information)
        {
        }

        protected TestBase(ITestOutputHelper outputHelper, LogLevel minLevel)
        {
            LoggerFactory = new TestLoggerFactory();
            Provider = new XunitLoggerProvider(outputHelper, minLevel);
            LoggerFactory.AddProvider(Provider);
            Logger = LoggerFactory.CreateLogger("Default");
        }
    }
}
