//HintName: Rocket.Surgery.Extensions.Testing.AutoFixtures/Rocket.Surgery.Extensions.Testing.AutoFixtures.AutoFixtureGenerator/AutoFixtureAttribute.g.cs
using System;
using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures
{
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional("CODEGEN")]
    internal class AutoFixtureAttribute : Attribute
    {
        public AutoFixtureAttribute(Type type) => Type = type;

        public Type Type { get; }
    }
}
