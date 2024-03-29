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
                   .WithId(Guid.Parse("1A9A7FD4-CEF0-4712-858C-685E8191FE4E"))
                   .AddReferences(typeof(Fake))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
                ClassSource,
                AttributedFixtureSource
            },
            new object[]
            {
                DefaultBuilder()
                   .WithId(Guid.Parse("5A24BB51-84DE-4688-829E-9FA0A49EF7F1"))
                   .AddReferences(typeof(Substitute))
                   .AddSources(ClassSource, AttributedFixtureSource)
                   .Build(),
                ClassSource,
                AttributedFixtureSource
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
