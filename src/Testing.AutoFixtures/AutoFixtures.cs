using System.Diagnostics;

namespace Rocket.Surgery.Extensions.Testing.AutoFixtures;

internal static class AutoFixtures
{
    public static string Version { get; } = FileVersionInfo.GetVersionInfo(typeof(AutoFixtureGenerator).Assembly.Location).ProductVersion;
    public static string CodeGenerationAttribute { get; } = $@"[System.CodeDom.Compiler.GeneratedCode(""AutoFixtures"", ""{Version}"")]";
}