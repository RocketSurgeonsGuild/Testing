using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Rocket.Surgery.Extensions.Testing.SourceGenerators;

internal static class GenerationHelpers
{
    public static Project CreateProject(
        string projectName,
        IEnumerable<MetadataReference> metadataReferences,
        CSharpParseOptions parseOptions,
        ImmutableArray<NamedSourceText> sources,
        ImmutableArray<AdditionalText> additionalTexts
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

        foreach (var item in additionalTexts)
        {
            if (item.GetText() is not { } source) continue;
            var documentId = DocumentId.CreateNewId(projectId, item.Path);
            solution = solution.AddAdditionalDocument(documentId, item.Path, source);
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