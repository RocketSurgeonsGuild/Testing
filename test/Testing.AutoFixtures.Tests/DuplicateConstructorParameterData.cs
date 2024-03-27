namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

public static class DuplicateConstructorParameterData
{
    public static IEnumerable<object[]> Data =>
        new List<object[]>
        {
            new object[] { ClassSource, AttributedFixtureSource },
        };

    private const string ClassSource =
        @"public class DuplicateConstructorParameter
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
}";

    private const string AttributedFixtureSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(DuplicateConstructorParameter))]
    internal sealed partial class DuplicateConstructorParameterFixture { }
}";
}

