using FluentAssertions;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public class AutoFixtureGeneratorTests
{
    [Fact]
    public async Task GivenAutoFixture_WhenGenerate_ThenShouldGenerateAutoFixtureAttribute()
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(List<>))
               .IgnoreOutputFile("AutoFixtureBase.g.cs")
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).ScrubLines(text => text.Contains("System.CodeDom.Compiler.GeneratedCode"));
    }

    [Fact]
    public async Task GivenAutoFixture_WhenGenerate_ThenShouldGenerateAutoFixtureBase()
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(List<>))
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    [MemberData(nameof(ParameterArraySourceData.Data), MemberType = typeof(ParameterArraySourceData))]
    [MemberData(nameof(ValueTypeSourceData.Data), MemberType = typeof(ValueTypeSourceData))]
    public async Task GivenAutoFixtureAttribute_WhenGenerate_ThenGeneratesAutoFixture(
        GeneratorTestContext context
    )
    {
        // Given, When
        var result =
            await context
               .GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(context.Id);
    }

    [Theory]
    [MemberData(nameof(ParameterArraySourceData.ParameterArrayDeck), MemberType = typeof(ParameterArraySourceData))]
    public async Task GivenConstructorWithParameterArray_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .Should()
           .Contain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Diagnostics.AutoFixture0001.Id));
    }
}