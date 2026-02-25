using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class RecordConstructorFixtureData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
    [
        DefaultBuilder()
           .AddSources(RecordSource, RecordFixture)
           .Build(),
    ];

    private const string RecordSource = @"
namespace Goony.Goo.Goo;

public record UserAccount(string UserId, string? LastLogin = null, string? ExpiringRefresh = null);
";
    private const string RecordFixture = @"
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests;

[AutoFixture(typeof(UserAccount))]
public record UserAccountFixture;
";
}
