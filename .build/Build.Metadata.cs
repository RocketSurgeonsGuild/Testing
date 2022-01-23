using System.Collections.Immutable;
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


public interface IParseGeneratorMetadata : IHaveSolution, IHaveOutputLogs, IHaveBuildTarget, IHaveRestoreTarget, IComprehendSources, IHaveGitVersion,
                                           IComprehendTests, IHaveTestTarget, IHavePackTarget
{
    public Target LoadProjectData => _ =>
        _
           .DependentFor(Pack)
           .Executes(
                () =>
                {
                    var projects = new List<GeneratorItem>();

                    projects.Add(new GeneratorItem("Rocket.Surgery.Extensions.Testing", ImmutableArray<PackageReferenceItem>.Empty));
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
                                                   z => new PackageReferenceItem(
                                                       z.Name,
                                                       lockFile.Libraries.First(x => x.Name == z.Name).Version
                                                   )
                                               )
                                              .ToList();


                        projects.Add(new GeneratorItem(lockFile.PackageSpec.Name, results.ToImmutableArray()));
                        if (lockFile.PackageSpec.Name == "Rocket.Surgery.Extensions.Testing.XUnit")
                        {
                            projects.Add(
                                new GeneratorItem(
                                    lockFile.PackageSpec.Name,
                                    results
                                       .Select(z => z with { Name = z.Name.Replace("xunit.abstractions", "xunit") })
                                       .ToImmutableArray()
                                )
                            );
                        }
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
                            "Condition", "'$(ManagePackageVersionsCentrally)' == 'true' and '$(ImplicitPackageReferences)' == 'true'"
                        );
                        implicitPackageReferencesTarget.Add(propertyGroup.Clone());

                        implicitCentralPackageVersionsTarget.SetAttributeValue("Name", "AddRsgImplicitCentralPackageVersions");
                        implicitCentralPackageVersionsTarget.SetAttributeValue("BeforeTargets", "CollectCentralPackageVersions");
                        implicitCentralPackageVersionsTarget.SetAttributeValue("AfterTargets", "CollectPackageReferences");
                        implicitCentralPackageVersionsTarget.SetAttributeValue(
                            "Condition", "'$(ManagePackageVersionsCentrally)' == 'true' and '$(ImplicitPackageReferences)' == 'true'"
                        );
                        implicitCentralPackageVersionsTarget.Add(propertyGroup.Clone());


                        var implicitTestingReferenceItemGroup = new XElement("ItemGroup");
                        implicitTestingReferenceItemGroup.SetAttributeValue(
                            "Condition", "'$(ManagePackageVersionsCentrally)' == 'true' and '$(ImplicitPackageReferences)' == 'true'"
                        );

                        var defaultPackageReference = new XElement("PackageReference");
                        defaultPackageReference.SetAttributeValue("Include", "Rocket.Surgery.Extensions.Testing");
                        implicitTestingReferenceItemGroup.Add(defaultPackageReference);
                        xProject.Add(implicitTestingReferenceItemGroup);

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
                            var prop = new XElement("ImplicitPackageReferences");
                            xProperties.Add(prop);
                            prop.SetValue("true");
                            prop.SetAttributeValue("Condition", "'$(ImplicitPackageReferences)' == ''");
                        }
                        {
                            var prop = new XElement("ImplicitPackageReferenceWarning");
                            xProperties.Add(prop);
                            prop.SetValue("true");
                            prop.SetAttributeValue("Condition", "'$(ImplicitPackageReferenceWarning)' == ''");
                        }
                    }

                    var addedItems = new HashSet<string>();
                    foreach (var project in projects)
                    {
                        var version = GitVersion.FullSemVer;

                        var conditionPropertyName = $"ImplicitPackageReference{project.AssemblyName.Replace(".", "")}";
                        if (!addedItems.Contains(project.AssemblyName))
                        {
                            var enabledProperty = new XElement(conditionPropertyName);
                            enabledProperty.SetValue("true");
                            enabledProperty.SetAttributeValue("Condition", $"'$({conditionPropertyName})' == ''");
                            xProperties.Add(enabledProperty);
                        }

                        var packageReferenceItemGroup = new XElement("ItemGroup");
                        var conditionBuilder = new StringBuilder();
                        conditionBuilder
                           .Append("'$(").Append(conditionPropertyName).Append(")' == 'true' ");
                        if (project.PackageReferences.Length > 0)
                        {
                            conditionBuilder
                               .Append("and ")
                               .AppendJoin(" and ", project.PackageReferences.Select(z => $"$(_rsgPackageReferenceList.Contains('{z.Name}'))"));
                        }

                        packageReferenceItemGroup.SetAttributeValue(
                            "Condition", conditionBuilder + $" and !$(_rsgPackageReferenceList.Contains('{project.AssemblyName}'))"
                        );

                        var packageReference = new XElement("PackageReference");
                        packageReferenceItemGroup.Add(packageReference);
                        packageReference.SetAttributeValue("Include", project.AssemblyName);
                        implicitPackageReferencesTarget.Add(packageReferenceItemGroup);

                        conditionBuilder.Append(" and !$(_rsgPackageVersionList.Contains('").Append(project.AssemblyName).Append("'))");

                        var packageVersionWarning = new XElement("Warning");
                        packageVersionWarning.SetAttributeValue("Condition", $"'$(ImplicitPackageReferenceWarning)' == 'true' and {conditionBuilder}");
                        packageVersionWarning.SetAttributeValue(
                            "Text",
                            $"PackageReference to {project.AssemblyName} has been added implicitly using default version {version}. Add <PackageVersion Include=\"{project.AssemblyName}\" Version=\"{version}\" /> or disable this warning with <ImplicitPackageReferenceWarning>false</ImplicitPackageReferenceWarning>.  Use <ImplicitPackageReferences>false</ImplicitPackageReferences> to disable all implicit package references or <{conditionPropertyName}>false</{conditionPropertyName}> to disable only this implicit reference."
                        );
                        var packageVersionItemGroup = new XElement("ItemGroup");
                        packageVersionItemGroup.SetAttributeValue("Condition", conditionBuilder);
                        var packageVersion = new XElement("PackageVersion");
                        packageVersion.SetAttributeValue("Include", project.AssemblyName);
                        packageVersion.SetAttributeValue("Version", version);
                        packageVersionItemGroup.Add(packageVersion);
                        implicitCentralPackageVersionsTarget.Add(packageVersionWarning);
                        implicitCentralPackageVersionsTarget.Add(packageVersionItemGroup);
                        addedItems.Add(project.AssemblyName);
                    }

                    // src/Testing/build/Rocket.Surgery.Extensions.Testing.props

                    propsDoc.Save(SourceDirectory / "Testing" / "Sdk" / "ImplicitPackageReferences.props");
                    targetsDoc.Save(SourceDirectory / "Testing" / "Sdk" / "ImplicitPackageReferences.targets");
                }
            );
}

public record GeneratorItem(string AssemblyName, ImmutableArray<PackageReferenceItem> PackageReferences);

public record PackageReferenceItem(string Name, NuGetVersion Version);
