//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Upsert.CommandHandler.AutoFixture.g.cs
using System.Collections.ObjectModel;
using FakeItEasy;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    internal sealed partial class DifferentNamedFixture : AutoFixtureBase<DifferentNamedFixture>
    {
        public static implicit operator Upsert.CommandHandler(DifferentNamedFixture fixture) => fixture.Build();
        public DifferentNamedFixture WithThing(Goony.Goo.Goo.IThing thing) => With(ref _thing, thing);
        private Upsert.CommandHandler Build() => new Upsert.CommandHandler(_thing);
        private Goony.Goo.Goo.IThing _thing = A.Fake<Goony.Goo.Goo.IThing>();
    }
}