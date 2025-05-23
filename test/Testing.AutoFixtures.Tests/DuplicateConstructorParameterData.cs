using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class DuplicateConstructorParameterData : AutoFixtureSourceData
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
    public class DuplicateConstructorParameter
    {
        public DuplicateConstructorParameter(int count, bool ready, double percentage, int range)
        {
        }

        public DuplicateConstructorParameter(int count, bool ready, double percentage)
        {
        }

        public DuplicateConstructorParameter(int count, bool ready)
        {
        }
    }
}";

    private const string AttributedFixtureSource = @"using System;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(DuplicateConstructorParameter))]
    internal sealed partial class DuplicateConstructorParameterFixture { }
}";
}
