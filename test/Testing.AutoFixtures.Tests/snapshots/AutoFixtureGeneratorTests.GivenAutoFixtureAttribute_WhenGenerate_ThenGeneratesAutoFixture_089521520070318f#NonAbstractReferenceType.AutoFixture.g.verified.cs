//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/NonAbstractReferenceType.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Tests.Goo.Goo
{
    internal sealed partial class NonAbstractReferenceTypeFixture : AutoFixtureBase<NonAbstractReferenceTypeFixture>
    {
        public static implicit operator NonAbstractReferenceType(NonAbstractReferenceTypeFixture fixture) => fixture.Build();
        public NonAbstractReferenceTypeFixture WithOne(Goony.Goo.Goo.Fish one) => With(ref _one, one);
        public NonAbstractReferenceTypeFixture WithTwo(Goony.Goo.Goo.Fish two) => With(ref _two, two);
        public NonAbstractReferenceTypeFixture WithRed(Goony.Goo.Goo.Color red) => With(ref _red, red);
        public NonAbstractReferenceTypeFixture WithBlue(Goony.Goo.Goo.Color blue) => With(ref _blue, blue);
        private NonAbstractReferenceType Build() => new NonAbstractReferenceType(_one, _two, _red, _blue);
        private Goony.Goo.Goo.Fish _one = default;
        private Goony.Goo.Goo.Fish _two = default;
        private Goony.Goo.Goo.Color _red = default;
        private Goony.Goo.Goo.Color _blue = default;
    }
}