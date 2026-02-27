//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/CollectionSupport.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class CollectionSupportFixture : AutoFixtureBase<CollectionSupportFixture>
    {
        public static implicit operator CollectionSupport(CollectionSupportFixture fixture) => fixture.Build();
        public CollectionSupportFixture WithStrings(IEnumerable<System.String> strings) => With(ref _strings, strings);
        public CollectionSupportFixture WithInts(IList<System.Int32> ints) => With(ref _ints, ints);
        public CollectionSupportFixture WithCollection(ICollection<System.Boolean> collection) => With(ref _collection, collection);
        private CollectionSupport Build() => new CollectionSupport(_strings, _ints, _collection);
        private IEnumerable<System.String> _strings = [];
        private IList<System.Int32> _ints = [];
        private ICollection<System.Boolean> _collection = [];
    }
}
