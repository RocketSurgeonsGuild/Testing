namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class Attribute
{
    public const string Source = @"using System;
using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixture
{
    [AttributeUsage(AttributeTargets.Class)]
    [Conditional(""CODEGEN"")]
    internal class AutoFixtureAttribute : Attribute
    {
        public AutoFixtureAttribute(Type type) => Type = type;

        public Type Type { get; }
    }
}";
}