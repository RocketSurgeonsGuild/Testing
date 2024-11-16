using Moq;

namespace Rocket.Surgery.Extensions.Testing;

public interface IAutoMockTestContext : ILoggingTestContext
{
    MockBehavior MockBehavior { get; }
}