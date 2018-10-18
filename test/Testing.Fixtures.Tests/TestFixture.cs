using System;
using System.Collections.Generic;

namespace Testing.Fixture.Tests
{
    public class TestFixture
    {
        public string Name { get; set; }

        public int Count { get; set; }

        public IEnumerable<string> Tests { get; set; }
    }
}
