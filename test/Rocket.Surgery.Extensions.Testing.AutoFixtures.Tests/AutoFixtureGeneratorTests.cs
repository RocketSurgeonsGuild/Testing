using FakeItEasy;
using Microsoft.CodeAnalysis;
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
               .IgnoreOutputFile("BuilderExtensions.cs")
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
               .IgnoreOutputFile("Attribute.cs")
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType= typeof(AutoFixtureGeneratorData))]
    public async Task GivenAutoFixtureAttributeUsage_WhenGenerate_ThenGeneratedAutoFixture(string source)
    {
        // Given
        GeneratorTestContext generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .IgnoreOutputFile("BuilderExtensions.cs")
               .IgnoreOutputFile("Attribute.cs")
               .AddSources(source)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType= typeof(AutoFixtureGeneratorData))]
    public async Task GivenFakeItEasy_WhenGenerate_ThenGeneratedAutoFixtureWithFakes(string source)
    {
        // Given
        GeneratorTestContext generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Fake))
               .IgnoreOutputFile("BuilderExtensions.cs")
               .IgnoreOutputFile("Attribute.cs")
               .AddSources(source)
               .Build();

        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType= typeof(AutoFixtureGeneratorData))]
    public async Task GivenNSubstitute_WhenGenerate_ThenGeneratedAutoFixtureWithFakes(string source)
    {
        // Given
        GeneratorTestContext generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .AddReferences(typeof(Substitute))
               .IgnoreOutputFile("BuilderExtensions.cs")
               .IgnoreOutputFile("Attribute.cs")
               .AddSources(source)
               .Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result).UseHashedParameters(source);
    }

//    [Fact]
    public async Task GivenAttributeOnClass_When_ThenShouldGenerateAutoFixture()
    {
        // Given
        GeneratorTestContext generatorInstance =
            GeneratorTestContextBuilder
               .Create()
               .WithGenerator<AutoFixtureGenerator>()
               .AddReferences(typeof(ILogger<>))
               .IgnoreOutputFile("BuilderExtensions.cs")
               .IgnoreOutputFile("Attribute.cs")
               .AddSources(@"using System;
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
                ).Build();


        // When
        var result = await generatorInstance.GenerateAsync();

        // Then
        await Verify(result);
    }
}