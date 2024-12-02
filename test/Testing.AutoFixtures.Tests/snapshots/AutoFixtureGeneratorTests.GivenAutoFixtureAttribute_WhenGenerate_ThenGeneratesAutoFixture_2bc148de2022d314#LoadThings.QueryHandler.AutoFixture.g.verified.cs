//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/LoadThings.QueryHandler.AutoFixture.g.cs
using System.Collections.ObjectModel;
using FakeItEasy;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    internal sealed partial class DifferentNamedFixture : AutoFixtureBase<DifferentNamedFixture>
    {
        public static implicit operator LoadThings.QueryHandler(DifferentNamedFixture fixture) => fixture.Build();
        public DifferentNamedFixture WithThing(Goony.Goo.Goo.IThing thing) => With(ref _thing, thing);
        private LoadThings.QueryHandler Build() => new LoadThings.QueryHandler(_thing);
        private Goony.Goo.Goo.IThing _thing = A.Fake<Goony.Goo.Goo.IThing>();
    }
}