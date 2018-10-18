using System.Collections.Generic;

namespace Rocket.Surgery.Extensions.Testing.Fixtures.Tests
{
    public class TestFixture
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public IEnumerable<string> Tests { get; set; }
    }
}
