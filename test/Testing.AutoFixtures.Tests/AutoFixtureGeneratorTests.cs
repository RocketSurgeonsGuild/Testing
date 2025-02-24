using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

[System.Diagnostics.DebuggerDisplay("{DebuggerDisplay,nq}")]
public class AutoFixtureGeneratorTests
{
    [System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]
    private string DebuggerDisplay
    {
        get
        {
            return ToString();
        }
    }

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
        _ = await Verify(result).ScrubLines(text => text.Contains("System.CodeDom.Compiler.GeneratedCode"));
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
        _ = await Verify(result);
    }

    [Theory]
    [MemberData(nameof(AutoFixtureGeneratorData.Data), MemberType = typeof(AutoFixtureGeneratorData))]
    [MemberData(nameof(DuplicateConstructorParameterData.Data), MemberType = typeof(DuplicateConstructorParameterData))]
    [MemberData(nameof(ParameterArraySourceData.Data), MemberType = typeof(ParameterArraySourceData))]
    [MemberData(nameof(ValueTypeSourceData.Data), MemberType = typeof(ValueTypeSourceData))]
    [MemberData(nameof(NonAbstractReferenceTypeData.Data), MemberType = typeof(NonAbstractReferenceTypeData))]
    [MemberData(nameof(UsingTypeNamespaceSourceData.Data), MemberType = typeof(UsingTypeNamespaceSourceData))]
    [MemberData(nameof(NestedClassFixtureData.Data), MemberType = typeof(NestedClassFixtureData))]
    [MemberData(nameof(DifferentNamedFixtureData.Data), MemberType = typeof(DifferentNamedFixtureData))]
    public async Task GivenAutoFixtureAttribute_WhenGenerate_ThenGeneratesAutoFixture(
        GeneratorTestContext context
    )
    {
        // Given, When
        var result =
            await context
               .GenerateAsync();

        // Then
        _ = await Verify(result).HashParameters().UseParameters(context.Id);
    }

    [Theory]
    [MemberData(nameof(GenerateMultipleFixturesData.Data), MemberType = typeof(GenerateMultipleFixturesData))]
    public async Task GivenNoConstructor_WhenGenerate_ThenReportsDiagnostic(GeneratorTestContext context)
    {
        // Given, When
        var result = await context.GenerateAsync();

        // Then
        result
           .Results
           .ShouldContain(pair => pair.Value.Diagnostics.All(diagnostic => diagnostic.Id == Diagnostics.AutoFixture0001.Id));
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
           .ShouldContain(pair => pair.Value.Diagnostics.Any(diagnostic => diagnostic.Id == Diagnostics.AutoFixture0002.Id));
    }
}
