namespace Rocket.Surgery.Extensions.Testing.Fixtures.Tests;

public class TestFixtureBuilder : ITestFixtureBuilder
{
    private int _count;
    private string? _name;
    private List<string>? _tests = new List<string>();
    private Dictionary<string, string> _variables = new Dictionary<string, string>();

    public static implicit operator TestFixture(TestFixtureBuilder builder)
    {
        return builder.Build();
    }

    public TestFixture Build()
    {
        return new TestFixture
        {
            Name = _name,
            Count = _count,
            Tests = _tests,
            Variables = _variables
        };
    }

    public TestFixtureBuilder WithCount(int count)
    {
        return this.With(ref _count, count);
    }

    public TestFixtureBuilder WithDictionary(Dictionary<string, string> variables)
    {
        return this.With(ref _variables, variables);
    }

    public TestFixtureBuilder WithKeyValue(KeyValuePair<string, string> single)
    {
        return this.With(ref _variables, single);
    }

    public TestFixtureBuilder WithKeyValue(string key, string value)
    {
        return this.With(ref _variables, key, value);
    }

    public TestFixtureBuilder WithName(string name)
    {
        return this.With(ref _name, name);
    }

    public TestFixtureBuilder WithTest(string test)
    {
        return this.With(ref _tests, test);
    }

    public TestFixtureBuilder WithTests(IEnumerable<string> tests)
    {
        return this.With(ref _tests, tests);
    }
}
