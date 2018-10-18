using System.Collections.Generic;
using Rocket.Surgery.Extensions.Testing.Fixtures;
using Rocket.Surgery.Extensions.Testing.Fixtures.Tests;

namespace Testing.Fixture.Tests
{
    public class TestFixtureBuilder : ITestFixtureBuilder
    {
        private string _name;
        private int _count;
        private List<string> _tests = new List<string>();

        public TestFixtureBuilder WithName(string name) => this.With(ref _name, name);

        public TestFixtureBuilder WithCount(int count) => this.With(ref _count, count);

        public TestFixtureBuilder WithTests(IEnumerable<string> tests) => this.With(ref _tests, tests);

        public TestFixtureBuilder WithTest(string test) => this.With(ref _tests, test);

        public TestFixture Build() => new TestFixture
        {
            Name = _name,
            Count = _count,
            Tests = _tests
        };

        public static implicit operator TestFixture(TestFixtureBuilder builder) => builder.Build();
    }
}
