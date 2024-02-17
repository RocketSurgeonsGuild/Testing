using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class GenerationHelpers
{
    public static Project CreateProject(
        string projectName,
        IEnumerable<MetadataReference> metadataReferences,
        CSharpParseOptions parseOptions,
        NamedSourceText[] sources
    )
    {
        var projectId = ProjectId.CreateNewId(projectName);
        var solution = new AdhocWorkspace()
                      .CurrentSolution
                      .AddProject(projectId, projectName, projectName, LanguageNames.CSharp)
                      .WithProjectCompilationOptions(
                           projectId,
                           new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                       )
                      .WithProjectParseOptions(projectId, parseOptions)
                      .AddMetadataReferences(projectId, metadataReferences);

        var count = 0;
        foreach (var source in sources)
        {
            var newFileName = source.Name ?? DefaultFilePathPrefix + count + "." + CSharpDefaultFileExt;
            if (!newFileName.EndsWith(CSharpDefaultFileExt))
            {
                newFileName += "." + CSharpDefaultFileExt;
            }

            var documentId = DocumentId.CreateNewId(projectId, newFileName);
            solution = solution.AddDocument(documentId, newFileName, source.SourceText);
            count++;
        }

        var project = solution.GetProject(projectId);
        if (project is null)
        {
            throw new InvalidOperationException($"The ad hoc workspace does not contain a project with the id {projectId.Id}");
        }

        return project;
    }

    internal const string DefaultFilePathPrefix = "Input";
    internal const string CSharpDefaultFileExt = "cs";
}

internal record NamedSourceText(SourceText SourceText)
{
    public string? Name { get; init; }
}