//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/Thing.AutoFixture.g.cs
using System;
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class ThingFixture : AutoFixtureBase<ThingFixture>
    {
        public static implicit operator Thing(ThingFixture fixture) => fixture.Build();
        public ThingFixture WithThingOne(System.Int32 thingOne) => With(ref _thingOne, thingOne);
        public ThingFixture WithThingTwo(Goony.Goo.Goo.Numbered thingTwo) => With(ref _thingTwo, thingTwo);
        public ThingFixture WithThingThree(Goony.Goo.Goo.Colored thingThree) => With(ref _thingThree, thingThree);
        private Thing Build() => new Thing(_thingOne, _thingTwo, _thingThree);
        private System.Int32 _thingOne = default;
        private Goony.Goo.Goo.Numbered _thingTwo = default;
        private Goony.Goo.Goo.Colored _thingThree = default;
    }
}