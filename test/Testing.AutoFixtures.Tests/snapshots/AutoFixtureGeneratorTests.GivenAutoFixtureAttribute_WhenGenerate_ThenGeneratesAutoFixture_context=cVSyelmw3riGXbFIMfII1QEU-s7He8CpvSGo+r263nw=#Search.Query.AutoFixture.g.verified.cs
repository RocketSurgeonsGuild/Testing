//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Search.Query.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    internal sealed partial class SearchQueryFixture : AutoFixtureBase<SearchQueryFixture>
    {
        public static implicit operator Search.Query(SearchQueryFixture fixture) => fixture.Build();
        public SearchQueryFixture WithBuilder(Func<Goony.Goo.Goo.Search.FilterBuilder,SearchFilter> Builder) => With(ref _Builder, Builder);
        private Search.Query Build() => new Search.Query(_Builder);
        private Func<Goony.Goo.Goo.Search.FilterBuilder,SearchFilter> _Builder = default;
    }
}
