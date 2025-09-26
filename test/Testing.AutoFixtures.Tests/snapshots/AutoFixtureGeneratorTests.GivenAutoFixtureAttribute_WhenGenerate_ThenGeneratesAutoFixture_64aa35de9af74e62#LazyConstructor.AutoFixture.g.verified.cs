//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/LazyConstructor.AutoFixture.g.cs
using System.Collections.ObjectModel;
using Goony.Goo.Goo;
using NSubstitute;
using Rocket.Surgery.Extensions.Testing.AutoFixtures;

namespace Goony.Goo.Goo.Tests
{
    internal sealed partial class LazyConstructorFixture : AutoFixtureBase<LazyConstructorFixture>
    {
        public static implicit operator LazyConstructor(LazyConstructorFixture fixture) => fixture.Build();
        public LazyConstructorFixture WithLazyThing(Lazy<Goony.Goo.Goo.IThing> lazyThing) => With(ref _lazyThing, lazyThing);
        private LazyConstructor Build() => new LazyConstructor(_lazyThing);
        private Lazy<Goony.Goo.Goo.IThing> _lazyThing = default;
    }
}