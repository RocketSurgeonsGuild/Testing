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
           .AnalyzerResults
           .ShouldContain(pair => pair.Value.Diagnostics.All(diagnostic => diagnostic.Id == Rsaf0002.Descriptor.Id));
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    [MemberData(nameof(Rsaf0002Data.DiagnosticReported), MemberType = typeof(Rsaf0002Data))]
    public async Task GivenConstructorWithoutParameterArray_WhenGenerate_ThenDoesNotReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .AnalyzerResults
           .ShouldNotContain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Rsaf0002.Descriptor.Id));
    }

    [Theory]
    [MemberData(nameof(Rsaf0002Data.DiagnosticReported), MemberType = typeof(Rsaf0002Data))]
    public async Task GivenDiagnosticReported_WhenGenerate_ThenGeneratesOtherFixtures(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        _ = await Verify(result).HashParameters().UseParameters(context.Id);
    }
}
