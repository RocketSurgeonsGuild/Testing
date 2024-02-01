using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
///     Defines a test that has access to the XUnit <see cref="ITestOutputHelper" /> attached
/// </summary>
public interface IHasTestOutputHelper
{
    /// <summary>
    ///     The test output helper
    /// </summary>
    ITestOutputHelper TestOutputHelper { get; }
}