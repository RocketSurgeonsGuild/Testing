using System.Diagnostics;
using System.Reflection;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class AutoFixtures
{
    public static string Version { get; } =
        typeof(AutoFixtureGenerator).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
     ?? typeof(AutoFixtureGenerator).Assembly.GetName().Version.ToString();

    public static string CodeGenerationAttribute { get; } = $@"[System.CodeDom.Compiler.GeneratedCode(""AutoFixtures"", ""{Version}"")]";
}
