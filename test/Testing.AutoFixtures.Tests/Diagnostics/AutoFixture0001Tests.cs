using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests.Diagnostics;

public class AutoFixture0001Tests
{
    [Theory]
    [MemberData(nameof(GenerateMultipleFixturesData.Data), MemberType = typeof(GenerateMultipleFixturesData))]
    public async Task GivenNoConstructor_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .ShouldContain(pair => pair.Value.Diagnostics.All(diagnostic => diagnostic.Id == AutoFixture0001.Descriptor.Id));
    }

    [Fact]
    public async Task GivenDiagnosticReported_WhenGenerate_ThenGeneratesOtherFixtures()
    {
        // Given

        // When

        // Then
    }
}
