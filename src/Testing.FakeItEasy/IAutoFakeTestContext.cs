using FakeItEasy.Creation;

namespace Rocket.Surgery.Extensions.Testing;

public interface IAutoFakeTestContext : ILoggingTestContext
{
    Action<IFakeOptions>? FakeOptionsAction { get; }
}