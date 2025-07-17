using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests.Diagnostics;

public class Rsaf0001Tests
{
    [Theory]
    [MemberData(nameof(NoConstructorData.Data), MemberType = typeof(NoConstructorData))]
    [MemberData(nameof(Rsaf0001Data.DiagnosticReported), MemberType = typeof(Rsaf0001Data))]
    public async Task GivenNoConstructor_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .ShouldContain(pair => pair.Value.Diagnostics.All(diagnostic => diagnostic.Id == Rsaf0001.Descriptor.Id));
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    [MemberData(nameof(NoConstructorData.Data), MemberType = typeof(NoConstructorData))]
    [MemberData(nameof(Rsaf0001Data.DiagnosticReported), MemberType = typeof(Rsaf0001Data))]
    public async Task GivenConstructor_WhenGenerate_ThenDoesNotReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .ShouldNotContain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Rsaf0001.Descriptor.Id));
    }

    [Theory]
    [MemberData(nameof(Rsaf0001Data.DiagnosticReported), MemberType = typeof(Rsaf0001Data))]
    public async Task GivenDiagnosticReported_WhenGenerate_ThenGeneratesOtherFixtures(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        _ = await Verify(result).HashParameters().UseParameters(context.Id);
    }
}
