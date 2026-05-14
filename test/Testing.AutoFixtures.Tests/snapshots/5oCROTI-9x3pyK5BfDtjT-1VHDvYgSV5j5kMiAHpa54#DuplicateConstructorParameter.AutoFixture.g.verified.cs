//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/DuplicateConstructorParameter.AutoFixture.g.cs
using System;
using System.Collections.ObjectModel;
using FakeItEasy;
using Goony.Goo.Goo;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class DuplicateConstructorParameterFixture : AutoFixtureBase<DuplicateConstructorParameterFixture>
    {
        public static implicit operator DuplicateConstructorParameter(DuplicateConstructorParameterFixture fixture) => fixture.Build();
        public DuplicateConstructorParameterFixture WithCount(System.Int32 count) => With(ref _count, count);
        public DuplicateConstructorParameterFixture WithReady(System.Boolean ready) => With(ref _ready, ready);
        public DuplicateConstructorParameterFixture WithPercentage(System.Double percentage) => With(ref _percentage, percentage);
        public DuplicateConstructorParameterFixture WithRange(System.Int32 range) => With(ref _range, range);
        private DuplicateConstructorParameter Build() => new DuplicateConstructorParameter(_count, _ready, _percentage, _range);
        private System.Int32 _count = default;
        private System.Boolean _ready = default;
        private System.Double _percentage = default;
        private System.Int32 _range = default;
    }
}