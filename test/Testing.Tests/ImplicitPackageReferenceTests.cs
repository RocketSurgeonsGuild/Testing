using System.Diagnostics;
using System.Xml.Linq;
using Buildalyzer;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace Rocket.Surgery.Extensions.Testing.Tests;

public class ImplicitPackageReferenceTests : LoggerTest
{
    public ImplicitPackageReferenceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public void Should_Add_XUnit(
    )
    {
        var (project, _) = GenerateProject("xunit.abstractions");
        var results = project.Build();
        results.OverallSuccess.Should().BeTrue();
    }

    private static string FixturesPath = Path.Combine(Path.GetDirectoryName(typeof(ImplicitPackageReferenceTests).Assembly.Location), "Fixtures");
    private static string RootPath;

    static ImplicitPackageReferenceTests()
    {
        var path = FixturesPath;
        while (Directory.Exists(path))
        {
            if (Directory.Exists(Path.Combine(path, ".nuke")))
            {
                break;
            }

            path = Path.GetDirectoryName(path);
        }

        RootPath = path;

        var info = new ProcessStartInfo("dotnet", "nuke SetupMagicProjectTestData")
        {
            WorkingDirectory = path
        };
        Process.Start(info)!.WaitForExit();
        File.Copy(Path.Combine(path!, "test/Testing.Tests/Fixtures/Directory.Packages.props"), Path.Combine(FixturesPath, "Directory.Packages.Props"), true);

        var directoryProps = Path.Combine(FixturesPath, "Directory.Build.Props");

        var document = XDocument.Load(directoryProps, LoadOptions.PreserveWhitespace);
        document.Descendants("RestoreAdditionalProjectSources").Single().SetValue(Path.Combine(path, "artifacts", "nuget") + Path.DirectorySeparatorChar);
        document.Descendants("RestorePackagesPath").Single().SetValue(Path.Combine(path, ".nuke", "temp", "packages") + Path.DirectorySeparatorChar);
        document.Save(directoryProps);
    }

    private (IProjectAnalyzer project, AnalyzerManager manager) GenerateProject(
        string dependency,
        bool ImplicitReferencePackages = true,
        bool ImplicitReferenceWarning = true,
        string otherProperties = ""
    )
    {
        var projectName = Guid.NewGuid().ToString("N");
        var projectPath = Path.Combine(FixturesPath, projectName);
        Directory.CreateDirectory(projectPath);


        var content = $@"<Project Sdk=""Microsoft.NET.Sdk"">
  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <ImplicitPackageReferencePackages>{( ImplicitReferencePackages ? "true" : "false" )}</ImplicitPackageReferencePackages>
    <ImplicitPackageReferenceWarning>{( ImplicitReferenceWarning ? "true" : "false" )}</ImplicitPackageReferenceWarning>
{otherProperties}
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include=""{dependency}"" />
    <PackageReference Include=""Rocket.Surgery.Extensions.Testing"" />
  </ItemGroup>
</Project>";

        var projectFilePath = Path.Combine(projectPath, $"{projectName}.csproj");
        File.WriteAllText(projectFilePath, content);

        var manager = new AnalyzerManager();
//        manager.SetGlobalProperty("RestoreAdditionalProjectSources", Path.Combine(RootPath, "artifacts", "nuget") + Path.DirectorySeparatorChar);
//        manager.SetGlobalProperty("RestorePackagesPath", Path.Combine(RootPath, ".tmp", "packages") + Path.DirectorySeparatorChar);

        var project = manager.GetProject(projectFilePath);

        return ( project, manager );
    }
}
