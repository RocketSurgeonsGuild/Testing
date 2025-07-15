using Microsoft.Extensions.Logging;
using Rocket.Surgery.Extensions.Testing.AutoFixtures.Diagnostics;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public abstract class AutoFixtureSourceData
{
    protected static GeneratorTestContextBuilder DefaultBuilder()
    {
        return GeneratorTestContextBuilder
              .Create()
              .WithGenerator<AutoFixtureGenerator>()
              .WithAnalyzer<AutoFixture0001>()
              .WithAnalyzer<AutoFixture0002>()
              .AddReferences(typeof(ILogger<>))
              .IgnoreOutputFile("AutoFixtureAttribute.g.cs")
              .IgnoreOutputFile("AutoFixtureBase.g.cs");
    }
}
