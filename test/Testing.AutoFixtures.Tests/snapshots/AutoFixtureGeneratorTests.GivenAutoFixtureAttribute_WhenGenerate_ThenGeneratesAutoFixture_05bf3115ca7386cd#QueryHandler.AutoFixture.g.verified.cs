//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/QueryHandler.AutoFixture.g.cs
using System.Collections.ObjectModel;
using FakeItEasy;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    internal sealed partial class LoadThingsQueryHandlerFixture : AutoFixtureBase<LoadThingsQueryHandlerFixture>
    {
        public static implicit operator LoadThings.QueryHandler(LoadThingsQueryHandlerFixture fixture) => fixture.Build();
        public LoadThingsQueryHandlerFixture WithThing(Goony.Goo.Goo.IThing thing) => With(ref _thing, thing);
        private LoadThings.QueryHandler Build() => new LoadThings.QueryHandler(_thing);
        private Goony.Goo.Goo.IThing _thing = A.Fake<Goony.Goo.Goo.IThing>();
    }
}