using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public class AutoFixtureGeneratorTests
{
    [Fact]
    public async Task Given_When_ThenShouldGenerateAutoFixtureAttribute()
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
    public async Task Given_When_ThenShouldGenerateFixtureBuilderExtensions()
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

    [Fact]
    public async Task Given_When_ThenShouldGenerateAutoFixture()
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
    [AutoFixture(typeof(Authenticator))]
    internal sealed partial class AuthenticatorFixture : ITestFixtureBuilder { }
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

    [Fact]
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