using FakeItEasy;
using Microsoft.Extensions.Logging;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     A simple logging factory to create a faked logger, wrapping the logger
/// </summary>
internal class FakeItEasyLoggerFactory : LoggerFactory, ILoggerFactory
{
    private readonly ILoggerFactory _factory;

    public FakeItEasyLoggerFactory(ILoggerFactory factory)
    {
        _factory = factory;
    }

    ILogger ILoggerFactory.CreateLogger(string categoryName)
    {
        var logger = _factory.CreateLogger(categoryName);
        return A.Fake<ILogger>(x => x.Wrapping(logger));
    }
}
