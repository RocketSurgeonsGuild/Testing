using Microsoft.Extensions.Logging;

using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal abstract class AutoFixtureSourceData
{
    protected static GeneratorTestContextBuilder DefaultBuilder() => GeneratorTestContextBuilder
        .Create()
        .WithGenerator<AutoFixtureGenerator>()
        .WithAnalyzer<Rsaf0001>()
        .WithAnalyzer<Rsaf0002>()
        .WithAnalyzer<Rsaf0003>()
        .AddReferences(typeof(ILogger<>))
        .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
        .IgnoreOutputFile("AutoFixtureBase.g.cs");
}
