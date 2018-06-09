using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing
{
    public class TestLoggerFactory : LoggerFactory, ILoggerFactory
    {
        ILogger ILoggerFactory.CreateLogger(string categoryName)
        {
            var logger = base.CreateLogger(categoryName);
            return A.Fake<ILogger>(x => x.Wrapping(logger));
        }
    }
}
