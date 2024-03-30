using Rocket.Surgery.Extensions.Testing.SourceGenerators;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures.Tests;

internal class ValueTypeSourceData : AutoFixtureSourceData
{
    public static TheoryData<GeneratorTestContext> Data =>
        new() { DefaultBuilder().AddSources(ClassSource).AddSources(AttributedSource).Build(), };

    private const string ClassSource = @"
namespace Goony.Goo.Goo
{
    public class Thing
    {
        public Thing(int thingOne, Numbered thingTwo, Colored thingThree)
        {
            ThingOne = thingOne;
            ThingTwo = thingTwo;
            ThingThree = thingThree;
        }

        public int ThingOne { get; }

        public Numbered ThingTwo { get; }

        public Colored ThingThree { get; }
    }

    public struct Numbered
    {
        public static Numbered One = new Numbered();

        public static Numbered Two = new Numbered();

        public Numbered() { }
    }

    public enum Colored
    {
        Red,

        Blue
    }
}";

    private const string AttributedSource = @"using System;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    [AutoFixture(typeof(Thing)]
    internal sealed partial class ThingFixture { }
}";
}
