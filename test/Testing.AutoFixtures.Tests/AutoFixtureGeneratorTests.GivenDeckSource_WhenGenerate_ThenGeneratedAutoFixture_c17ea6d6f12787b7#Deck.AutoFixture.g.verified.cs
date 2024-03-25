﻿//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Deck.AutoFixture.g.cs
using NSubstitute;
using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class DeckFixture : ITestFixtureBuilder
    {
        public static implicit operator Deck(DeckFixture fixture) => fixture.Build();
        public DeckFixture WithEnumerable(System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card> cards) => this.With(ref _cards, cards);
        public DeckFixture With<TField>(ref TField field, TField value)
        {
            field = value;
            return this;
        }

        private Deck Build() => new Deck(_cards);
        private System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card> _cards = Substitute.For<System.Collections.Generic.IEnumerable<Goony.Goo.Goo.Card>>();
    }
}