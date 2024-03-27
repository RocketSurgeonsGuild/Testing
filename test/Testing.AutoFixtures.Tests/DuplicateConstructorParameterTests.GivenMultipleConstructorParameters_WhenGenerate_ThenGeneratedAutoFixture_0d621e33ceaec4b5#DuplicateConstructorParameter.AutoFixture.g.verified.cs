//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/DuplicateConstructorParameter.AutoFixture.g.cs
using System.Collections.ObjectModel;
using System;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class DuplicateConstructorParameterFixture : AutoFixtureBase<DuplicateConstructorParameterFixture>
    {
        public static implicit operator DuplicateConstructorParameter(DuplicateConstructorParameterFixture fixture) => fixture.Build();
        public DuplicateConstructorParameterFixture WithInt32(System.Int32 count) => With(ref _count, count);
        public DuplicateConstructorParameterFixture WithBoolean(System.Boolean ready) => With(ref _ready, ready);
        public DuplicateConstructorParameterFixture WithDouble(System.Double percentage) => With(ref _percentage, percentage);
        public DuplicateConstructorParameterFixture WithInt32(System.Int32 range) => With(ref _range, range);
        private DuplicateConstructorParameter Build() => new DuplicateConstructorParameter(_count, _ready, _percentage, _range);
        private System.Int32 _count = Substitute.For<System.Int32>();
        private System.Boolean _ready = Substitute.For<System.Boolean>();
        private System.Double _percentage = Substitute.For<System.Double>();
        private System.Int32 _range = Substitute.For<System.Int32>();
    }
}