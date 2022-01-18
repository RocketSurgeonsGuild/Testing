using System.Xml.Serialization;
using NuGet.LibraryModel;
using NuGet.ProjectModel;
using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

public partial class NukeSolution : IParseGeneratorMetadata
{
}


public interface IParseGeneratorMetadata : IHaveSolution, IHaveOutputLogs, IHaveBuildTarget, IHaveRestoreTarget, IComprehendSources
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
                                                       MajorVersion = lockFile.Libraries.First(x => x.Name == z.Name).Version.Major
                                                   }
                                               )
                                              .ToList();


                        projects.Add(
                            new GeneratorItem
                            {
                                AssemblyName = lockFile.PackageSpec.Name,
                                Files = project.Directory
                                               .GlobFiles("**/*.cs")
                                               .Select(z => project.Directory.GetRelativePathTo(z.Parent / z.Name).ToString())
                                               .Where(z => !( z.StartsWith("obj/") || z.StartsWith("obj\\") || z.StartsWith("bin/") || z.StartsWith("bin\\") ))
                                               .ToList(),
                                PackageReferences = results
                            }
                        );
                    }

                    var serializer = new XmlSerializer(typeof(List<GeneratorItem>));
                    var path = Solution.GetProject("Rocket.Surgery.Extensions.Testing.Analyzers")!.Directory / "data.xml";
                    if (path.FileExists())
                    {
                        FileSystemTasks.DeleteFile(path);
                    }

                    using var file = File.Open(path, FileMode.CreateNew);
                    serializer.Serialize(file, projects);
                }
            );
}

[Serializable]
public class GeneratorItem
{
//    [XmlAttribute]
    public string AssemblyName { get; init; }
    [XmlElement(Type = typeof(string))] public List<string> Files { get; set; }
    public List<PackageReferenceItem> PackageReferences { get; init; }
}

[Serializable]
public class PackageReferenceItem
{
    public string Name { get; set; }
    public int MajorVersion { get; set; }
}
