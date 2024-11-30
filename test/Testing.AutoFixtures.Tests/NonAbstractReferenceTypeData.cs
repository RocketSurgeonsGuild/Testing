using FakeItEasy;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class NonAbstractReferenceTypeData : AutoFixtureSourceData
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
            DefaultBuilder()
               .AddReferences(typeof(Fake))
               .AddSources(AttributedSource)
               .Build(),
            DefaultBuilder()
               .AddReferences(typeof(Substitute))
               .AddSources(AttributedSource)
               .Build(),
        };

    private const string ClassSource = @"
namespace Goony.Goo.Goo
{
    public class NonAbstractReferenceType
    {
        public NonAbstractReferenceType(Fish one, Fish two, Color red, Color blue)
        {
            One = one;
            Two = two;
            Red = red;
            Blue = blue;
        }

        public Fish One { get; }
        public Fish Two { get; }
        public Color Red { get; }
        public Color Blue { get; }
    }

    public class Fish { }

    public class Color { }
}";

    private const string AttributedSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    [AutoFixture]
    public class NonAbstractReferenceType
    {
        public NonAbstractReferenceType(Fish one, Fish two, Color red, Color blue)
        {
            One = one;
            Two = two;
            Red = red;
            Blue = blue;
        }

        public Fish One { get; }
        public Fish Two { get; }
        public Color Red { get; }
        public Color Blue { get; }
    }

    public class Fish { }

    public class Color { }
}";

    private const string AttributedFixtureSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo
{
    [AutoFixture(typeof(NonAbstractReferenceType))]
    internal partial class NonAbstractReferenceTypeFixture
    {
    }
}";
}
