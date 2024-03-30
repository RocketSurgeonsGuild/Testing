using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class ParameterArraySourceData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
        new()
        {
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(EnumerableDeckSource, CardsDomainSource, DeckAutoFixtureAttributedSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(EnumerableDeckSource, CardsDomainSource, DeckAutoFixtureAttributedSource)
               .Build(),
        };

    public static TheoryData<GeneratorTestContext> ParameterArrayDeck =>
        new()
        {
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(ParamsArrayDeckSource, CardsDomainSource, DeckAutoFixtureAttributedSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(ParamsArrayDeckSource, CardsDomainSource, DeckAutoFixtureAttributedSource)
               .Build(),
        };

    private const string ParamsArrayDeckSource = @"using System;
using System.Collections.Generic;
using System.Linq;

namespace Goony.Goo.Goo
{
    public class Deck : IReadOnlyCollection<Card>
    {
        public Deck(params Card[] cards) => _cards = cards.ToList();

        public Deck[] Deal() => new Deck[] { };

        public void EndRound(IEnumerable<Card> cards) => _cards.AddRange(cards);

        /// <inheritdoc/>
        public IEnumerator<Card> GetEnumerator() => _cards.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _cards.Count;

        private List<Card> _cards;
    }
}";


    private const string EnumerableDeckSource = @"using System;
using System.Collections.Generic;
using System.Linq;

namespace Goony.Goo.Goo
{
    public class Deck : IReadOnlyCollection<Card>
    {
        public Deck(IEnumerable<Card> cards) => _cards = cards.ToList();

        public Deck[] Deal() => new Deck[] { };

        public void EndRound(IEnumerable<Card> cards) => _cards.AddRange(cards);

        /// <inheritdoc/>
        public IEnumerator<Card> GetEnumerator() => _cards.GetEnumerator();

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _cards.Count;

        private List<Card> _cards;
    }
}";

    private const string CardsDomainSource = @"using System;
using System.Collections.Generic;
using System.Linq;

namespace Goony.Goo.Goo
{
    public class Card
    {
        public Card(Rank rank, Suit suit)
        {
            Rank = rank;
            Suit = suit;
        }

        public Rank Rank { get; set; }

        public Suit Suit { get; set; }
    }

    public struct Suit
    {
        private Suit(string suit)
        {
            Value = suit;
        }

        public string Value;

        public static Suit Spade { get; } = new Suit(nameof(Spade));

        public static Suit Club { get; } = new Suit(nameof(Club));

        public static Suit Heart { get; } = new Suit(nameof(Heart));

        public static Suit Diamond { get; } = new Suit(nameof(Diamond));
    }

    public enum Rank
    {
        Two = 2,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Jack,
        Queen,
        King,
        Ace
    }
}";

    private const string DeckAutoFixtureAttributedSource = @"using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Deck))]
    internal partial class DeckFixture {}
}";
}