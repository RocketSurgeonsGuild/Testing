//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Deck.AutoFixture.g.cs
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FakeItEasy;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class DeckFixture : AutoFixtureBase<DeckFixture>
    {
        public static implicit operator Deck(DeckFixture fixture) => fixture.Build();
        public DeckFixture WithEnumerable(System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card> cards) => With(ref _cards, cards);
        private Deck Build() => new Deck(_cards);
        private System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card> _cards = A.Fake<System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card>>();
    }
}