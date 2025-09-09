//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Upsert.CommandHandler.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    internal sealed partial class UpsertCommandHandlerFixture : AutoFixtureBase<UpsertCommandHandlerFixture>
    {
        public static implicit operator Upsert.CommandHandler(UpsertCommandHandlerFixture fixture) => fixture.Build();
        public UpsertCommandHandlerFixture WithThing(Goony.Goo.Goo.IThing thing) => With(ref _thing, thing);
        private Upsert.CommandHandler Build() => new Upsert.CommandHandler(_thing);
        private Goony.Goo.Goo.IThing _thing = Substitute.For<Goony.Goo.Goo.IThing>();
    }
}