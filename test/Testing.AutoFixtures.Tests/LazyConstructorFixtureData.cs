using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class LazyConstructorFixtureData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
    [
        DefaultBuilder()
           .AddReferences(typeof(Fake))
           .AddSources(Source, IThing, AttributedFixtureSource)
           .Build(),
        DefaultBuilder()
           .AddReferences(typeof(Substitute))
           .AddSources(Source, IThing, AttributedFixtureSource)
           .Build(),
    ];
    // lang=csharp
    private const string Source =
        """
        namespace Goony.Goo.Goo;

        public class LazyConstructor
        {
            public LazyConstructor(Lazy<IThing> lazyThing) { }
        }
        """;

    // lang=csharp
    private const string AttributedFixtureSource =
        """
        using Rocket.Surgery.Extensions.Testing.AutoFixtures;

        namespace Goony.Goo.Goo.Tests;

        [AutoFixture(typeof(LazyConstructor))]
        public class LazyConstructorFixture;
        """;

    // lang=csharp
    private const string IThing =
        """
        namespace Goony.Goo.Goo;

        public interface IThing;
        """;
}
