using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class CollectionExpressionFixtureData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
    [
        DefaultBuilder()
           .AddSources(RecordSource, RecordFixture)
           .Build(),
    ];

    private const string RecordSource = @"
namespace Goony.Goo.Goo;

public class CollectionSupport
{
    public CollectionSupport(IEnumerable<string> strings, IList<int> ints, ICollection<bool> collection)
    {
        Strings = strings;
        Ints = ints;
        Collection = collection;
    }

    public IEnumerable<string> Strings { get; }
    public IList<int> Ints { get; }
    public ICollection<bool> Collection { get; }
}
";
    private const string RecordFixture = @"
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests;

[AutoFixture(typeof(CollectionSupport))]
public record CollectionSupportFixture;
";
}
