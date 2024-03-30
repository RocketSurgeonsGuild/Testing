using FakeItEasy;
using NSubstitute;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class DuplicateConstructorParameterData : AutoFixtureSourceData
{
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[]
            {
                DefaultBuilder()
                   .AddReferences(typeof(Fake))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
            },
            new object[]
            {
                DefaultBuilder()
                   .AddReferences(typeof(Substitute))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
            },
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
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(DuplicateConstructorParameter))]
    internal sealed partial class DuplicateConstructorParameterFixture { }
}";
}
