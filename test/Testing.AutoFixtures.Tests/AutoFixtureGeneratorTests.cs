using FluentAssertions;
using Microsoft.Extensions.Logging;
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
        await Verify(result);
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
    public async Task GivenAutoFixtureAttribute_WhenGenerate_ThenGeneratesAutoFixture(
        GeneratorTestContext context
    )
    {
        // Given, When
        var result =
            await context
                 .GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(context.ToString());
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

    [Theory]
    [MemberData(nameof(ParameterArraySourceData.EnumerableDeck), MemberType = typeof(ParameterArraySourceData))]
    public async Task GivenConstructorWithEnumerable_WhenGenerate_ThenGeneratesAutoFixture(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(context.ToString());
    }

//    [Fact]
    public async Task GivenAttributeOnClass_When_ThenShouldGenerateAutoFixture()
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .IgnoreOutputFile("BuilderExtensions.cs")
               .IgnoreOutputFile("Attribute.cs")
               .AddSources(
                    @"using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixture;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture]
    internal class Authenticator
    {
        public Authenticator(IAuthenticationClient authenticationClient,
            ISecureStorage secureStorage,
            ILogger<Authenticator> logger) {}
    }
    internal interface ISecureStorage {}
    internal interface IAuthenticationClient {}
}"
                )
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result);
    }
}
