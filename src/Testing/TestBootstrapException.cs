namespace Rocket.Surgery.Extensions.Testing;

/// <summary>
/// Exception when trying to boot strap the tests
/// </summary>
public class TestBootstrapException : Exception
{
    /// <inheritdoc />
    public TestBootstrapException(string message, Exception innerException) : base(message, innerException)
    {
    }

    /// <inheritdoc />
    public TestBootstrapException(string message) : base(message)
    {
    }

    /// <inheritdoc />
    public TestBootstrapException()
    {
    }
}
