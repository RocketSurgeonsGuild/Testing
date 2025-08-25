using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests.Diagnostics;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
internal class Rsaf0003Tests
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

    [Theory]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    public async Task GivenNoConstructor_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .AnalyzerResults
           .ShouldContain(pair => pair.Value.Diagnostics.All(diagnostic => diagnostic.Id == Rsaf0003.Descriptor.Id));
    }

    [Theory]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    public async Task GivenDiagnosticReported_WhenGenerate_ThenGeneratesOtherFixtures(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        _ = await Verify(result).HashParameters().UseParameters(context.Id);
    }
}
