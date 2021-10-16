#pragma warning disable CA2227 // Collection properties should be read only

namespace Rocket.Surgery.Extensions.Testing.Fixtures.Tests;

public class TestFixture
{
    public int Count { get; set; }

    public string? Name { get; set; }

    public IEnumerable<string>? Tests { get; set; }

    public Dictionary<string, string> Variables { get; set; } = new Dictionary<string, string>();
}