using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    /// <summary>
    /// A simple logging factory to create a faked logger, wrapping the logger
    /// </summary>
    public class TestLoggerFactory : LoggerFactory, ILoggerFactory
    {
        ILogger ILoggerFactory.CreateLogger(string categoryName)
        {
            var logger = CreateLogger(categoryName);
            return A.Fake<ILogger>(x => x.Wrapping(logger));
        }
    }
}
