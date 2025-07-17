using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests.Diagnostics;

public class Rsaf0002Tests
{
    [Theory]
    [MemberData(nameof(ParameterArraySourceData.ParameterArrayDeck), MemberType = typeof(ParameterArraySourceData))]
    public async Task GivenConstructorWithParameterArray_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .ShouldContain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Rsaf0002.Descriptor.Id));
    }

    [Fact]
    public async Task GivenDiagnosticReported_WhenGenerate_ThenGeneratesOtherFixtures()
    {
        // Given

        // When

        // Then
    }
}
