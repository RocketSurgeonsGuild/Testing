using FakeItEasy;
using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Logging;
using NSubstitute;
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
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result);
    }

    [Fact]
    public async Task GivenAutoFixture_WhenGenerate_ThenShouldGenerateFixtureBuilderExtensions()
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
    public async Task GivenAutoFixtureAttributeUsage_WhenGenerate_ThenGeneratedAutoFixture(string source)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(source)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    public async Task GivenFakeItEasy_WhenGenerate_ThenGeneratedAutoFixtureWithFakes(string source)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Fake))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(source)
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    public async Task GivenNSubstitute_WhenGenerate_ThenGeneratedAutoFixtureWithFakes(string source)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(source)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.SeparateSource), MemberType = typeof(AutoFixtureGeneratorData))]
    public async Task GivenSeparateNamespace_WhenGenerate_ThenGeneratedAutoFixtureWithFakes(string classSource, string fixtureSource)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(classSource, fixtureSource)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(classSource, fixtureSource);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.ParameterArrayDeck), MemberType = typeof(AutoFixtureGeneratorData))]
    public async Task GivenDeckSource_WhenGenerate_ThenReportsDiagnostic(string deckSource, string cardSource, string fixtureSource)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(deckSource, cardSource, fixtureSource)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        result
           .Results
           .Should()
           .Contain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Diagnostics.AutoFixture0001.Id));
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.EnumerableDeck), MemberType = typeof(AutoFixtureGeneratorData))]
    public async Task GivenDeckSource_WhenGenerate_ThenGeneratedAutoFixture(string deckSource, string cardSource, string fixtureSource)
    {
        // Given
        var generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderInterface.g.cs")
               .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
               .AddSources(cardSource, deckSource, fixtureSource)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(deckSource, cardSource, fixtureSource);
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
