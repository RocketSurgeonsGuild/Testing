namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class Attribute
{
    public static string Source() => @$"#nullable enable
using System;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures
{{
    {AutoFixtures.CodeGenerationAttribute}
    [AttributeUsage(AttributeTargets.Class)]
    internal class AutoFixtureAttribute : Attribute
    {{
        public AutoFixtureAttribute() : this(null) {{}}

        public AutoFixtureAttribute(Type? type) => Type = type;

        public Type? Type {{ get; }}
    }}
}}";
}
