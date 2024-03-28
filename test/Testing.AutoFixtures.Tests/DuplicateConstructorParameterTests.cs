using Microsoft.Extensions.Logging;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public class DuplicateConstructorParameterTests
{
    [Theory]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    public async Task GivenMultipleConstructorParameters_WhenGenerate_ThenGeneratedAutoFixture(string classSource, string fixtureSource)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .IgnoreOutputFile("AutoFixtureBase.g.cs")
               .AddSources(classSource, fixtureSource)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(classSource, fixtureSource);
    }

    [Theory]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    public async Task GivenMultiplePrimitiveConstructorParameters_WhenGenerate_ThenGeneratedByParameterName(string classSource, string fixtureSource)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .IgnoreOutputFile("AutoFixtureBase.g.cs")
               .AddSources(classSource, fixtureSource)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(classSource, fixtureSource);
    }
}