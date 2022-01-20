using System.Text;
using System.Xml.Linq;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using NuGet.Versioning;
using Nuke.Common;
using Nuke.Common.ProjectModel;
using Nuke.Common.Utilities;

public partial class NukeSolution : IParseGeneratorMetadata
{
}

/*
 <Project>
    <Target Name="AddRsgImplicitPackageReferences" BeforeTargets="CollectPackageReferences" Condition="$(ManagePackageVersionsCentrally) == 'true'">
        <PropertyGroup>
            <_rsgPackageReferenceList>@(PackageReference)</_rsgPackageReferenceList>
            <_rsgPackageVersionList>@(PackageVersion)</_rsgPackageVersionList>
        </PropertyGroup>

        <ItemGroup Condition="$(_rsgPackageReferenceList.Contains('Microsoft.Extensions.Logging')) and $(_rsgPackageReferenceList.Contains('Serilog'))">
            <PackageReference Include="Serilog.Extensions.Logging" />
        </ItemGroup>
    </Target>
    <Target Name="AddRsgImplicitCentralPackageVersions" BeforeTargets="CollectCentralPackageVersions" AfterTargets="CollectPackageReferences">
        <PropertyGroup>
            <_rsgPackageReferenceList>@(PackageReference)</_rsgPackageReferenceList>
            <_rsgPackageVersionList>@(PackageVersion)</_rsgPackageVersionList>
        </PropertyGroup>

        <Warning Text="PackageReference to Serilog.Extensions.Logging has been implicitly added but the PackageVersion is missing using default 3.1.0" Condition="$(_rsgPackageReferenceList.Contains('Microsoft.Extensions.Logging')) and $(_rsgPackageReferenceList.Contains('Serilog')) and !$(_rsgPackageVersionList.Contains('Serilog.Extensions.Logging'))" />

        <ItemGroup Condition="$(_rsgPackageReferenceList.Contains('Microsoft.Extensions.Logging')) and $(_rsgPackageReferenceList.Contains('Serilog')) and !$(_rsgPackageVersionList.Contains('Serilog.Extensions.Logging'))">
            <PackageVersion Include="Serilog.Extensions.Logging" Version="3.1.0" />
        </ItemGroup>
    </Target>
</Project>
 */


public interface IParseGeneratorMetadata : IHaveSolution, IHaveOutputLogs, IHaveBuildTarget, IHaveRestoreTarget, IComprehendSources, IHaveGitVersion
{
    public Target LoadProjectData => _ =>
        _
           .Before(Build)
           .DependsOn(Restore)
           .TriggeredBy(Restore)
           .Executes(
                () =>
                {
                    var projects = new List<GeneratorItem>();
                    var lockFileFormat = new LockFileFormat();
                    foreach (var project in Solution
                                           .AllProjects
                                           .Where(z => z.GetProperty<bool?>("IsMagicProject") == true))
                    {
                        var lockFile = lockFileFormat.Read(project.Directory / "obj" / "project.assets.json")!;
                        var results = lockFile.PackageSpec.TargetFrameworks
                                              .SelectMany(
                                                   z => z.Dependencies
                                                         .Where(
                                                              z => !z.AutoReferenced
                                                                && ( z.IncludeType & LibraryIncludeFlags.Compile ) != 0
                                                                && z.ReferenceType == LibraryDependencyReferenceType.Direct
                                                          ),
                                                   (information, dependency) => ( target: information.TargetAlias, dependency )
                                               )
                                              .GroupBy(z => z.dependency.Name)
                                              .Where(z => z.Count() == lockFile.PackageSpec.TargetFrameworks.Count)
                                              .Select(z => z.First().dependency)
                                              .Select(
                                                   z => new PackageReferenceItem
                                                   {
                                                       Name = z.Name,
                                                       Version = lockFile.Libraries.First(x => x.Name == z.Name).Version
                                                   }
                                               )
                                              .ToList();


                        projects.Add(
                            new GeneratorItem
                            {
                                AssemblyName = lockFile.PackageSpec.Name,
                                PackageReferences = results
                            }
                        );
                    }

                    var targetsDoc = new XDocument();
                    var implicitPackageReferencesTarget = new XElement("Target");
                    var implicitCentralPackageVersionsTarget = new XElement("Target");
                    var xProperties = new XElement("PropertyGroup");
                    {
                        var xProject = new XElement("Project");
                        var propertyGroup = XElement.Parse(
                            @"<PropertyGroup><_rsgPackageReferenceList>@(PackageReference)</_rsgPackageReferenceList><_rsgPackageVersionList>@(PackageVersion)</_rsgPackageVersionList></PropertyGroup>"
                        );

                        implicitPackageReferencesTarget.SetAttributeValue("Name", "AddRsgImplicitPackageReferences");
                        implicitPackageReferencesTarget.SetAttributeValue("BeforeTargets", "CollectPackageReferences");
                        implicitPackageReferencesTarget.SetAttributeValue(
                            "Condition", "'$(ManagePackageVersionsCentrally)' == 'true' and '$(ImplicitReferencePackages)' == 'true'"
                        );
                        implicitPackageReferencesTarget.Add(propertyGroup.Clone());

                        implicitCentralPackageVersionsTarget.SetAttributeValue("Name", "AddRsgImplicitCentralPackageVersions");
                        implicitCentralPackageVersionsTarget.SetAttributeValue("BeforeTargets", "CollectCentralPackageVersions");
                        implicitCentralPackageVersionsTarget.SetAttributeValue("AfterTargets", "CollectPackageReferences");
                        implicitCentralPackageVersionsTarget.SetAttributeValue(
                            "Condition", "'$(ManagePackageVersionsCentrally)' == 'true' and '$(ImplicitReferencePackages)' == 'true'"
                        );
                        implicitCentralPackageVersionsTarget.Add(propertyGroup.Clone());

                        xProject.Add(implicitPackageReferencesTarget);
                        xProject.Add(implicitCentralPackageVersionsTarget);
                        targetsDoc.Add(xProject);
                    }
                    var propsDoc = new XDocument();
                    {
                        var xProject = new XElement("Project");
                        propsDoc.Add(xProject);
                        xProject.Add(xProperties);
                        {
                            var prop = new XElement("ImplicitReferencePackages");
                            xProperties.Add(prop);
                            prop.SetValue("true");
                            prop.SetAttributeValue("Condition", "'$(ImplicitReferencePackages)' == ''");
                        }
                        {
                            var prop = new XElement("ImplicitReferenceWarning");
                            xProperties.Add(prop);
                            prop.SetValue("true");
                            prop.SetAttributeValue("Condition", "'$(ImplicitReferenceWarning)' == ''");
                        }
                    }

                    foreach (var project in projects)
                    {
                        if (!project.PackageReferences.Any()) continue;

                        var version = GitVersion.NuGetVersionV2;

                        var conditionPropertyName = $"ImplicitReference{project.AssemblyName.Replace(".", "")}";
                        var enabledProperty = new XElement(conditionPropertyName);
                        enabledProperty.SetValue("true");
                        enabledProperty.SetAttributeValue("Condition", $"'$({conditionPropertyName})' == ''");
                        xProperties.Add(enabledProperty);

                        var packageReferenceItemGroup = new XElement("ItemGroup");
                        var conditionBuilder = new StringBuilder();
                        conditionBuilder.Append("'$(ImplicitReferencePackages)' == 'true' and ")
                                        .Append("'$(").Append(conditionPropertyName).Append(")' == 'true' and ")
                                        .Append(
                                             string.Join(" and ", project.PackageReferences.Select(z => $"$(_rsgPackageReferenceList.Contains('{z.Name}'))"))
                                         );
                        packageReferenceItemGroup.SetAttributeValue("Condition", conditionBuilder);

                        var packageReference = new XElement("PackageReference");
                        packageReferenceItemGroup.Add(packageReference);
                        packageReference.SetAttributeValue("Include", project.AssemblyName);
                        implicitPackageReferencesTarget.Add(packageReferenceItemGroup);

                        conditionBuilder.Append(" and !$(_rsgPackageVersionList.Contains('").Append(project.AssemblyName).Append("'))");

                        var packageVersionWarning = new XElement("Warning");
                        packageVersionWarning.SetAttributeValue("Condition", $"'$(ImplicitReferenceWarning)' == 'true' and {conditionBuilder}");
                        packageVersionWarning.SetAttributeValue(
                            "Text",
                            $"PackageReference to {project.AssemblyName} has been added implicitly using default version {version}. Add <PackageVersion Include=\"{project.AssemblyName}\" Version=\"{version}\" /> or disable this warning with <ImplicitReferenceWarning>false</ImplicitReferenceWarning>.  Use <ImplicitReferencePackages>false</ImplicitReferencePackages> to disable all implicit package references or <{conditionPropertyName}>false</{conditionPropertyName}> to disable only this implicit reference."
                        );
                        var packageVersionItemGroup = new XElement("ItemGroup");
                        packageReferenceItemGroup.SetAttributeValue("Condition", $"{conditionBuilder}");
                        var packageVersion = new XElement("PackageVersion");
                        packageVersion.SetAttributeValue("Include", project.AssemblyName);
                        packageVersion.SetAttributeValue("Version", version);
                        packageVersionItemGroup.Add(packageVersion);
                        implicitCentralPackageVersionsTarget.Add(packageVersionWarning);
                        implicitCentralPackageVersionsTarget.Add(packageVersionItemGroup);
                    }

                    // src/Testing/build/Rocket.Surgery.Extensions.Testing.props


                    propsDoc.Save(SourceDirectory / "Testing" / "build" / "ImplicitReferences.props");
                    targetsDoc.Save(SourceDirectory / "Testing" / "build" / "ImplicitReferences.targets");
                }
            );
}

public class GeneratorItem
{
//    [XmlAttribute]
    public string AssemblyName { get; init; }
    public List<PackageReferenceItem> PackageReferences { get; init; }
}

public class PackageReferenceItem
{
    public string Name { get; set; }
    public NuGetVersion Version { get; set; }
}
