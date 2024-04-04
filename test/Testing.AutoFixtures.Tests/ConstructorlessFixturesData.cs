using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public class GenerateMultipleFixturesData : AutoFixtureSourceData
{

    public static TheoryData<GeneratorTestContext> Data =>
        new()
        {
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(ClassSource, AttributedFixtureSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(ClassSource, AttributedFixtureSource)
               .Build(),
        };

    private const string ClassSource =
        @"namespace Goony.Goo.Goo
{
    public class Stuff
    {
        public string ThingOne { get; set; }
        public string ThingTwo { get; set; }
    }
}";

    private const string AttributedFixtureSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Stuff))]
    internal partial class StufFixture
    {
    }
}";
}
